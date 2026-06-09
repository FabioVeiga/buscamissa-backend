using BuscaMissa.Context;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.DTOs.v2.IgrejaDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using BuscaMissa.Services.v1;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services.v2;

public class IgrejaService(
    ApplicationDbContext context,
    ImagemService imagemService,
    ILogger<IgrejaService> logger
    )
{
    public async Task<Igreja?> TemNomeNomeUnicoAsync(string nomeUnico)
    {
        try
        {
            return await context.Igrejas
                .Include(igreja => igreja.Endereco)
                .Include(x => x.Usuario)
                .Include(x => x.Missas)
                .Include(igreja => igreja.Contato)
                .Include(igreja => igreja.RedesSociais)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.NomeUnico != null && x.NomeUnico == nomeUnico.ToLower());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting InformacoesGeraisResponse");
            throw;
        }
    }
    
    public async Task<Igreja> InserirAsync(PostIgrejaRequest request)
    {
        try
        {
            Igreja model = (Igreja)request;
            context.Igrejas.Add(model);
            await context.SaveChangesAsync();
            return model;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while insering Igreja {IgrejaRequest}", request);
            throw;
        }
    }
    
    public async Task<IList<Igreja>> BuscarPorCepAsync(string cep)
    {
        try
        {
            var model = await context.Igrejas
                .Include(igreja => igreja.Endereco)
                .Include(x => x.Usuario)
                .Include(x => x.Missas)
                .Include(igreja => igreja.Contato)
                .Include(igreja => igreja.RedesSociais)
                .AsNoTracking()
                .Where(x => x.Endereco.Cep == CepHelper.FormatarCep(cep))
                .ToListAsync();
            return model;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching Igreja with CEP {Cep}", cep);
            throw;
        }
    }
    
    public async Task<Paginacao<IgrejaResponse>> BuscarPorFiltros(FiltroIgrejaV2Request filtro)
        {
            try
            {
                var query = context.Igrejas
                .Include(x => x.Endereco)
                .Include(x => x.Usuario)
                .Include(Igreja => Igreja.Contato)
                .Include(Igreja => Igreja.RedesSociais)
                .Include(x => x.Denuncia)
                .AsNoTracking()
                .Where(x =>
                    x.Endereco.Uf == filtro.Uf.ToUpper()
                    && x.Ativo == filtro.Ativo)
                .AsQueryable();

                if (!string.IsNullOrEmpty(filtro.Localidade))
                    query = query.Where(x => x.Endereco.Localidade == filtro.Localidade);

                if (filtro.Bairro is not null && filtro.Bairro.Count > 0)
                    query = query.Where(x => filtro.Bairro.Contains(x.Endereco.Bairro));

                if (!string.IsNullOrEmpty(filtro.Nome))
                    query = query.Where(x => x.Nome.ToUpper().Contains(filtro.Nome.ToUpper()));

                if (filtro.DiaDaSemana is not null)
                    query = query.Where(x => x.Missas.Any(y => y.DiaSemana == filtro.DiaDaSemana));

                if (filtro.Horarios.Count > 0)
                    query = query.Where(x => x.Missas.Any(y => filtro.HorarioMissa.Contains(y.Horario)));


                var aux = query.Select(x => new IgrejaResponse()
                {
                    Id = x.Id,
                    Nome = x.Nome,
                    NomeUnico = x.NomeUnico,
                    Slug = x.Slug,
                    Endereco = (EnderecoIgrejaResponse)x.Endereco,
                    Usuario = x.Usuario == null ? null : (UsuarioDtoResponse)x.Usuario,
                    Alteracao = x.Alteracao,
                    Ativo = x.Ativo,
                    Criacao = x.Criacao,
                    ImagemUrl = x.ImagemUrl == null ? null: imagemService.ObterUrlAzureBlob($"igreja/{x.ImagemUrl!}"),
                    Paroco = x.Paroco,
                    Missas = x.Missas.Select(m => new MissaResponse
                    {
                        Id = m.Id,
                        DiaSemana = m.DiaSemana,
                        Horario = m.Horario.ToString(),
                        Observacao = m.Observacao,
                        FontePrincipal = m.FontePrincipal,
                        UltimaValidacao = m.UltimaValidacao
                        // StatusConfianca calculado em memória após materialização
                    }).ToList(),
                    Contato = x.Contato == null ? null : (IgrejaContatoResponse)x.Contato,
                    RedesSociais = x.RedesSociais == null ? Array.Empty<IgrejaRedesSociaisResponse>() : x.RedesSociais.Select(r => (IgrejaRedesSociaisResponse)r).ToList(),
                    Denuncia = x.Denuncia == null ? null : string.IsNullOrEmpty(x.Denuncia.AcaoRealizada) ? (DenunciarIgrejaAdminResponse)x.Denuncia : null
                });

                var resultado = await aux.PaginacaoAsync(filtro.Paginacao.PageIndex, filtro.Paginacao.PageSize);

                // Preencher confiança em memória (com fallback em Igreja.Alteracao se missa sem dados)
                foreach (var ig in resultado.Items)
                {
                    DateTime? fallback = ig.Usuario != null ? ig.Alteracao : null;
                    foreach (var m in ig.Missas)
                        ConfiancaCalculator.PreencherConfianca(m, fallback);
                    ig.StatusConfianca = ConfiancaCalculator.CalcularParaIgreja(ig.Missas);
                }

                return resultado;
            }
            catch (Exception)
            {

                throw;
            }
        }

    public async Task<IgrejaResponse?> ObterPorIdAsync(int id)
    {
        try
        {
            var entity = await context.Igrejas.FirstOrDefaultAsync(x => x.Id == id && x.Ativo);
            if (entity == null) return null;
            return (IgrejaResponse)entity;

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while searching Igreja {IgrejaRequest}", id);
            throw;
        }
    }

    public async Task<IgrejaResponse?> BuscarPorNomeUnicoAsync(string nomeUnico)
    {
        try
        {
            var model = await context.Igrejas
                .Include(x => x.Endereco)
                .Include(x => x.Missas)
                .Include(x => x.Usuario)
                .Include(x => x.Contato)
                .Include(x => x.RedesSociais)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.NomeUnico == nomeUnico.ToLower() && x.Ativo);

            if (model == null) return null;

            var response = (IgrejaResponse)model;
            if (!string.IsNullOrEmpty(model.ImagemUrl))
                response.ImagemUrl = imagemService.ObterUrlAzureBlob($"igreja/{model.ImagemUrl}");

            // Preencher confiança em memória
            DateTime? fallback = model.Usuario != null ? model.Alteracao : null;
            foreach (var m in response.Missas)
                ConfiancaCalculator.PreencherConfianca(m, fallback);
            response.StatusConfianca = ConfiancaCalculator.CalcularParaIgreja(response.Missas);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching Igreja with NomeUnico {NomeUnico}", nomeUnico);
            throw;
        }
    }

    // Página de cidade SEO: lista completa (sem paginação), com cap de segurança.
    // Paginar fragmentaria o conteúdo e prejudicaria a indexação. O cap protege
    // o caso extremo (ex: SP capital). A solução de escala é sub-dividir por bairro.
    private const int LimiteCidade = 300;

    public async Task<IList<IgrejaResponse>> BuscarPorCidadeAsync(string uf, string cidadeSlug)
    {
        try
        {
            // No banco: filtra e aplica o cap pegando os mais recentemente atualizados
            // (proxy de relevância, ordenável por índice). Confiança é reordenada em memória.
            var model = await context.Igrejas
                .Include(x => x.Endereco)
                .Include(x => x.Missas)
                .Include(x => x.Usuario)
                .Include(x => x.Contato)
                .Include(x => x.RedesSociais)
                .AsNoTracking()
                .Where(x => x.Ativo
                    && x.Endereco.Uf == uf.ToUpper()
                    && x.Endereco.CidadeSlug == cidadeSlug.ToLower())
                .OrderByDescending(x => x.Alteracao)
                .Take(LimiteCidade)
                .ToListAsync();

            var responses = new List<IgrejaResponse>();
            foreach (var m in model)
            {
                var r = (IgrejaResponse)m;
                if (!string.IsNullOrEmpty(m.ImagemUrl))
                    r.ImagemUrl = imagemService.ObterUrlAzureBlob($"igreja/{m.ImagemUrl}");

                DateTime? fallback = m.Usuario != null ? m.Alteracao : null;
                foreach (var miss in r.Missas)
                    ConfiancaCalculator.PreencherConfianca(miss, fallback);
                r.StatusConfianca = ConfiancaCalculator.CalcularParaIgreja(r.Missas);

                responses.Add(r);
            }

            // Exibição: mais confiáveis primeiro, depois alfabético
            return responses
                .OrderByDescending(r => r.StatusConfianca)
                .ThenBy(r => r.Nome)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching Igrejas for cidade {Uf}/{Cidade}", uf, cidadeSlug);
            throw;
        }
    }

    // Paróquia individual por cidade + slug (página /paroquia/{uf}/{cidade}/{slug})
    public async Task<IgrejaResponse?> BuscarPorCidadeESlugAsync(string uf, string cidadeSlug, string slug)
    {
        try
        {
            var model = await context.Igrejas
                .Include(x => x.Endereco)
                .Include(x => x.Missas)
                .Include(x => x.Usuario)
                .Include(x => x.Contato)
                .Include(x => x.RedesSociais)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Ativo
                    && x.Endereco.Uf == uf.ToUpper()
                    && x.Endereco.CidadeSlug == cidadeSlug.ToLower()
                    && x.Slug == slug.ToLower());

            if (model == null) return null;

            var response = (IgrejaResponse)model;
            if (!string.IsNullOrEmpty(model.ImagemUrl))
                response.ImagemUrl = imagemService.ObterUrlAzureBlob($"igreja/{model.ImagemUrl}");

            DateTime? fallback = model.Usuario != null ? model.Alteracao : null;
            foreach (var m in response.Missas)
                ConfiancaCalculator.PreencherConfianca(m, fallback);
            response.StatusConfianca = ConfiancaCalculator.CalcularParaIgreja(response.Missas);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching Igreja {Uf}/{Cidade}/{Slug}", uf, cidadeSlug, slug);
            throw;
        }
    }

    // SEO da página de cidade
    public SeoMetadataResponse GerarSeoMetadataCidade(
        string cidadeNome, string uf, string cidadeSlug, int total, string frontendBaseUrl)
    {
        return new SeoMetadataResponse
        {
            Title = $"Missas em {cidadeNome} - {uf} | Horários de Missa | BuscaMissa",
            Description = $"Horários de missa em {cidadeNome}/{uf}. {total} paróquia(s) com endereço, contato e horários. Encontre a missa mais perto de você.",
            CanonicalUrl = $"{frontendBaseUrl.TrimEnd('/')}/missas/{uf.ToLower()}/{cidadeSlug}",
            Keywords = $"missa {cidadeNome}, horário de missa {cidadeNome}, igreja católica {cidadeNome}, missa hoje {cidadeNome}, missa domingo {cidadeNome}, {uf}",
            OgImage = null
        };
    }

    public async Task<IList<SitemapIgrejaDto>> ObterDadosSitemapAsync()
    {
        try
        {
            return await context.Igrejas
                .AsNoTracking()
                .Where(x => x.Ativo && x.NomeUnico != null)
                .Select(x => new SitemapIgrejaDto { NomeUnico = x.NomeUnico!, Alteracao = x.Alteracao })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while generating sitemap data");
            throw;
        }
    }

    public SeoMetadataResponse GerarSeoMetadata(IgrejaResponse igreja, string frontendBaseUrl)
    {
        var diasDescricao = igreja.Missas.Any()
            ? "Horários: " + string.Join(", ", igreja.Missas.Select(m => $"{m.DiaSemana} às {m.Horario}"))
            : "Consulte os horários de missa.";

        // Canonical aponta para a nova URL /paroquia/{uf}/{cidade}/{slug}.
        // Fallback para /igrejas/{nomeUnico} se o registro ainda não tiver slug/cidadeSlug (pré-backfill).
        var temUrlNova = !string.IsNullOrEmpty(igreja.Endereco.CidadeSlug) && !string.IsNullOrEmpty(igreja.Slug);
        var canonical = temUrlNova
            ? $"{frontendBaseUrl.TrimEnd('/')}/paroquia/{igreja.Endereco.Uf.ToLower()}/{igreja.Endereco.CidadeSlug}/{igreja.Slug}"
            : $"{frontendBaseUrl.TrimEnd('/')}/igrejas/{igreja.NomeUnico}";

        return new SeoMetadataResponse
        {
            Title = $"Missas em {igreja.Nome} - {igreja.Endereco.Localidade}/{igreja.Endereco.Uf} | BuscaMissa",
            Description = $"{igreja.Nome}, {igreja.Endereco.Bairro}, {igreja.Endereco.Localidade}/{igreja.Endereco.Uf}. {diasDescricao}",
            CanonicalUrl = canonical,
            Keywords = $"missa, {igreja.Nome}, {igreja.Endereco.Localidade}, {igreja.Endereco.Uf}, horário de missa, {igreja.Endereco.Bairro}",
            OgImage = string.IsNullOrEmpty(igreja.ImagemUrl) ? null : igreja.ImagemUrl
        };
    }
}