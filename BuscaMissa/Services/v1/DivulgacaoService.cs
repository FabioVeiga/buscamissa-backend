using BuscaMissa.Context;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.DTOs.v1.DivulgacaoDto;
using BuscaMissa.DTOs.v1.EmailEventoIgrejaDto;
using BuscaMissa.DTOs.v1.EmailHtmlGenerator;
using BuscaMissa.Enums;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services.v1;

public class DivulgacaoService(
    ApplicationDbContext context,
    EmailService emailService,
    EmailEventoIgrejaService emailEventoIgrejaService,
    ILogger<DivulgacaoService> logger,
    IConfiguration configuration)
{
    private string FrontendBaseUrl => configuration["FrontendBaseUrl"] ?? "https://buscamissa.com.br";

    public async Task<DivulgacaoDashboardResponse> ObterDashboardAsync()
    {
        var totalIgrejas = await context.Igrejas.AsNoTracking().CountAsync(x => x.Ativo);

        return new DivulgacaoDashboardResponse
        {
            TotalIgrejas = totalIgrejas,
            SemContatoEmail = await AplicarModo(ModoDivulgacaoEnum.SemContatoEmail).CountAsync(),
            SemEmailAlteracaoPendente = await AplicarModo(ModoDivulgacaoEnum.SemEmailAlteracaoPendente).CountAsync(),
            SemContatoFacebook = await AplicarModo(ModoDivulgacaoEnum.SemContatoFacebook).CountAsync(),
            SemContatoInstagram = await AplicarModo(ModoDivulgacaoEnum.SemContatoInstagram).CountAsync(),
        };
    }

    public async Task<Paginacao<IgrejaDivulgacaoResponse>> BuscarIgrejasAsync(FiltroIgrejaDivulgacaoRequest filtro)
    {
        var query = AplicarModo(filtro.Modo);

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
            query = query.Where(x => x.Nome.Contains(filtro.Nome));

        if (!string.IsNullOrWhiteSpace(filtro.Cidade))
            query = query.Where(x => x.Endereco.Localidade.Contains(filtro.Cidade));

        if (!string.IsNullOrWhiteSpace(filtro.Uf))
            query = query.Where(x => x.Endereco.Uf == filtro.Uf);

        var total = await query.CountAsync();

        var itens = await query
            .OrderBy(x => x.Nome)
            .Skip((filtro.PageIndex - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .Select(x => new IgrejaDivulgacaoResponse
            {
                Id = x.Id,
                Nome = x.Nome,
                Cidade = x.Endereco.Localidade,
                CidadeSlug = x.Endereco.CidadeSlug,
                Uf = x.Endereco.Uf,
                Slug = x.Slug,
                Email = x.Contato != null ? x.Contato.EmailContato : null,
                Instagram = x.RedesSociais!
                    .Where(r => r.TipoRedeSocial == TipoRedeSocialEnum.Instagram)
                    .Select(r => r.NomeDoPerfil)
                    .FirstOrDefault(),
                Facebook = x.RedesSociais!
                    .Where(r => r.TipoRedeSocial == TipoRedeSocialEnum.Facebook)
                    .Select(r => r.NomeDoPerfil)
                    .FirstOrDefault(),
                Criacao = x.Criacao,
                Alteracao = x.Alteracao,
                UltimoContatoEmail = context.EmailEventosIgreja
                    .Where(e => e.IgrejaId == x.Id && e.Canal == CanalContatoEnum.Email && e.Enviado)
                    .OrderByDescending(e => e.DataEnvio)
                    .Select(e => e.DataEnvio)
                    .FirstOrDefault(),
                UltimoContatoFacebook = context.EmailEventosIgreja
                    .Where(e => e.IgrejaId == x.Id && e.Canal == CanalContatoEnum.Facebook && e.Enviado)
                    .OrderByDescending(e => e.DataEnvio)
                    .Select(e => e.DataEnvio)
                    .FirstOrDefault(),
                UltimoContatoInstagram = context.EmailEventosIgreja
                    .Where(e => e.IgrejaId == x.Id && e.Canal == CanalContatoEnum.Instagram && e.Enviado)
                    .OrderByDescending(e => e.DataEnvio)
                    .Select(e => e.DataEnvio)
                    .FirstOrDefault(),
            })
            .ToListAsync();

        return new Paginacao<IgrejaDivulgacaoResponse>(filtro.PageIndex, filtro.PageSize, total, itens);
    }

    private IQueryable<Igreja> AplicarModo(ModoDivulgacaoEnum modo)
    {
        var query = context.Igrejas
            .AsNoTracking()
            .Where(x => x.Ativo);

        return modo switch
        {
            ModoDivulgacaoEnum.SemContatoEmail => query
                .Where(x => x.Contato != null && !string.IsNullOrWhiteSpace(x.Contato.EmailContato))
                .Where(x => !context.EmailEventosIgreja.Any(e =>
                    e.IgrejaId == x.Id && e.Canal == CanalContatoEnum.Email && e.Enviado)),

            ModoDivulgacaoEnum.SemEmailAlteracaoPendente => query
                .Where(x => x.Alteracao > x.Criacao)
                .Where(x => x.Contato != null && !string.IsNullOrWhiteSpace(x.Contato.EmailContato))
                .Where(x => !context.EmailEventosIgreja.Any(e =>
                    e.IgrejaId == x.Id &&
                    e.Tipo == TipoEmailEventoIgrejaEnum.Alteracao &&
                    e.Enviado &&
                    e.DataEnvio >= x.Alteracao)),

            ModoDivulgacaoEnum.SemContatoFacebook => query
                .Where(x => x.RedesSociais!.Any(r => r.TipoRedeSocial == TipoRedeSocialEnum.Facebook))
                .Where(x => !context.EmailEventosIgreja.Any(e =>
                    e.IgrejaId == x.Id && e.Canal == CanalContatoEnum.Facebook && e.Enviado)),

            ModoDivulgacaoEnum.SemContatoInstagram => query
                .Where(x => x.RedesSociais!.Any(r => r.TipoRedeSocial == TipoRedeSocialEnum.Instagram))
                .Where(x => !context.EmailEventosIgreja.Any(e =>
                    e.IgrejaId == x.Id && e.Canal == CanalContatoEnum.Instagram && e.Enviado)),

            _ => query
        };
    }

    public async Task<bool> EnviarEmailAsync(Igreja igreja, bool criacao)
    {
        var emailContato = igreja.Contato?.EmailContato;
        var html = string.Empty;
        var assunto = string.Empty;
        var tipoEvento = criacao
            ? TipoEmailEventoIgrejaEnum.Criacao
            : TipoEmailEventoIgrejaEnum.Alteracao;

        try
        {
            if (string.IsNullOrWhiteSpace(emailContato))
                return false;

            var url = string.Concat(
                FrontendBaseUrl,
                "/paroquia/",
                igreja.Endereco.Uf.ToLower(),
                "/",
                igreja.Endereco.CidadeSlug,
                "/",
                igreja.Slug
            );

            if (criacao)
            {
                assunto = $"#Busca Missa - Cadastro da igreja {igreja.Nome}";

                html = EmailHtmlGenerator.GerarHtmlEmailCriacao(
                    igreja.Nome,
                    igreja.Endereco.Logradouro,
                    igreja.Endereco.Numero,
                    igreja.Endereco.Bairro,
                    igreja.Endereco.Localidade,
                    igreja.Endereco.Estado,
                    igreja.Paroco,
                    url
                );
            }
            else
            {
                assunto = $"#Busca Missa - Atualização das informações da igreja {igreja.Nome}";

                html = EmailHtmlGenerator.GerarHtmlEmailAlteracao(
                    igreja.Nome,
                    igreja.Endereco.Logradouro,
                    igreja.Endereco.Numero,
                    igreja.Endereco.Bairro,
                    igreja.Endereco.Localidade,
                    igreja.Endereco.Estado,
                    igreja.Paroco,
                    url
                );
            }

            var responseEmail = await emailService.EnviarEmail(
                [emailContato],
                assunto,
                html
            );

            var enviado = !string.IsNullOrWhiteSpace(responseEmail);

            await emailEventoIgrejaService.InserirAsync(new CriarEmailEventoIgrejaRequest
            {
                IgrejaId = igreja.Id,
                Tipo = tipoEvento,
                Assunto = assunto,
                EmailDestino = emailContato,
                NomeDestino = igreja.Nome,
                Html = html,
                Ativo = true,
                Enviado = enviado,
                DataEnvio = enviado ? DateTime.UtcNow : null,
                Observacao = enviado
                    ? "E-mail enviado automaticamente pelo fluxo administrativo."
                    : "Tentativa automática de envio realizada, porém o serviço de e-mail não retornou confirmação."
            });

            if (!enviado)
            {
                logger.LogWarning(
                    "Igreja processada com sucesso, mas o e-mail não foi enviado. IgrejaId: {IgrejaId}, Email: {Email}",
                    igreja.Id,
                    emailContato
                );
            }

            return enviado;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Igreja processada com sucesso, mas ocorreu erro ao enviar ou registrar e-mail. IgrejaId: {IgrejaId}",
                igreja.Id
            );

            if (!string.IsNullOrWhiteSpace(emailContato))
            {
                try
                {
                    await emailEventoIgrejaService.InserirAsync(new CriarEmailEventoIgrejaRequest
                    {
                        IgrejaId = igreja.Id,
                        Tipo = tipoEvento,
                        Assunto = string.IsNullOrWhiteSpace(assunto)
                            ? $"Notificação da igreja {igreja.Nome}"
                            : assunto,
                        EmailDestino = emailContato,
                        NomeDestino = igreja.Nome,
                        Html = html,
                        Ativo = true,
                        Enviado = false,
                        DataEnvio = null,
                        Observacao = $"Erro ao enviar ou registrar e-mail: {ex.Message}"
                    });
                }
                catch (Exception eventoEx)
                {
                    logger.LogError(
                        eventoEx,
                        "Erro ao registrar falha de evento de e-mail. IgrejaId: {IgrejaId}",
                        igreja.Id
                    );
                }
            }

            return false;
        }
    }

    public async Task<EnviarEmailLoteResponse> EnviarEmailLoteAsync(EnviarEmailLoteRequest request)
    {
        var criacao = request.Tipo.Contains("criacao", StringComparison.OrdinalIgnoreCase);

        var igrejas = await context.Igrejas
            .Include(x => x.Contato)
            .Include(x => x.Endereco)
            .Where(x => request.IgrejaIds.Contains(x.Id))
            .ToListAsync();

        var response = new EnviarEmailLoteResponse
        {
            TotalSolicitado = request.IgrejaIds.Count
        };

        foreach (var igreja in igrejas)
        {
            if (string.IsNullOrWhiteSpace(igreja.Contato?.EmailContato))
            {
                response.Falhas.Add(new EnvioLoteFalhaResponse
                {
                    IgrejaId = igreja.Id,
                    Nome = igreja.Nome,
                    Motivo = "Igreja não possui e-mail de contato cadastrado."
                });
                continue;
            }

            var enviado = await EnviarEmailAsync(igreja, criacao);
            if (enviado)
            {
                response.TotalEnviado++;
            }
            else
            {
                response.Falhas.Add(new EnvioLoteFalhaResponse
                {
                    IgrejaId = igreja.Id,
                    Nome = igreja.Nome,
                    Motivo = "Falha ao enviar e-mail. Veja os logs para mais detalhes."
                });
            }
        }

        return response;
    }
}
