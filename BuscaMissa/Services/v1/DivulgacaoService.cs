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

    private string ConstruirLinkPagina(Igreja igreja) => string.Concat(
        FrontendBaseUrl,
        "/paroquia/",
        igreja.Endereco.Uf.ToLower(),
        "/",
        igreja.Endereco.CidadeSlug,
        "/",
        igreja.Slug
    );

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

            var url = ConstruirLinkPagina(igreja);

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

    /// <summary>
    /// Mesmo e-mail de contato em várias igrejas (diocese, secretaria paroquial etc.) —
    /// envia um único e-mail com a lista de todas, mas registra um EmailEventoIgreja por
    /// igreja (mesma DataEnvio), pra não quebrar os filtros/contadores que são por igreja.
    /// </summary>
    private async Task<bool> EnviarEmailMultiploAsync(IList<Igreja> igrejas, bool criacao)
    {
        var emailContato = igrejas[0].Contato?.EmailContato;
        var tipoEvento = criacao ? TipoEmailEventoIgrejaEnum.Criacao : TipoEmailEventoIgrejaEnum.Alteracao;
        var nomesGrupo = string.Join(", ", igrejas.Select(x => x.Nome));

        var assunto = criacao
            ? $"#Busca Missa - Cadastro de {igrejas.Count} igrejas"
            : $"#Busca Missa - Atualização das informações de {igrejas.Count} igrejas";

        var itens = igrejas
            .Select(igreja => (igreja.Nome, ConstruirLinkPagina(igreja)))
            .ToList();

        var html = EmailHtmlGenerator.GerarHtmlEmailMultiplasIgrejas(itens, criacao);

        bool enviado;
        try
        {
            var responseEmail = await emailService.EnviarEmail([emailContato!], assunto, html);
            enviado = !string.IsNullOrWhiteSpace(responseEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao enviar e-mail consolidado para {Email} ({Total} igrejas)", emailContato, igrejas.Count);
            enviado = false;
        }

        foreach (var igreja in igrejas)
        {
            try
            {
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
                        ? $"E-mail consolidado enviado junto com: {nomesGrupo}."
                        : "Tentativa de envio de e-mail consolidado realizada, porém o serviço de e-mail não retornou confirmação."
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao registrar evento de e-mail consolidado. IgrejaId: {IgrejaId}", igreja.Id);
            }
        }

        return enviado;
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

        var semEmail = igrejas.Where(x => string.IsNullOrWhiteSpace(x.Contato?.EmailContato)).ToList();
        foreach (var igreja in semEmail)
        {
            response.Falhas.Add(new EnvioLoteFalhaResponse
            {
                IgrejaId = igreja.Id,
                Nome = igreja.Nome,
                Motivo = "Igreja não possui e-mail de contato cadastrado."
            });
        }

        // Mesmo e-mail cadastrado em várias igrejas do lote → um único e-mail consolidado
        // em vez de um por igreja (evita spam pro mesmo contato, ex: diocese/secretaria).
        var grupos = igrejas
            .Where(x => !string.IsNullOrWhiteSpace(x.Contato?.EmailContato))
            .GroupBy(x => x.Contato!.EmailContato!.Trim().ToLowerInvariant());

        foreach (var grupo in grupos)
        {
            var igrejasDoGrupo = grupo.ToList();
            var enviado = igrejasDoGrupo.Count > 1
                ? await EnviarEmailMultiploAsync(igrejasDoGrupo, criacao)
                : await EnviarEmailAsync(igrejasDoGrupo[0], criacao);

            if (enviado)
            {
                response.TotalEnviado += igrejasDoGrupo.Count;
            }
            else
            {
                foreach (var igreja in igrejasDoGrupo)
                {
                    response.Falhas.Add(new EnvioLoteFalhaResponse
                    {
                        IgrejaId = igreja.Id,
                        Nome = igreja.Nome,
                        Motivo = "Falha ao enviar e-mail. Veja os logs para mais detalhes."
                    });
                }
            }
        }

        return response;
    }
}
