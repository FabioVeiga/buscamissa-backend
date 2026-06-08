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

        return new SeoMetadataResponse
        {
            Title = $"Missas em {igreja.Nome} - {igreja.Endereco.Localidade}/{igreja.Endereco.Uf} | BuscaMissa",
            Description = $"{igreja.Nome}, {igreja.Endereco.Bairro}, {igreja.Endereco.Localidade}/{igreja.Endereco.Uf}. {diasDescricao}",
            CanonicalUrl = $"{frontendBaseUrl.TrimEnd('/')}/igrejas/{igreja.NomeUnico}",
            Keywords = $"missa, {igreja.Nome}, {igreja.Endereco.Localidade}, {igreja.Endereco.Uf}, horário de missa, {igreja.Endereco.Bairro}",
            OgImage = string.IsNullOrEmpty(igreja.ImagemUrl) ? null : igreja.ImagemUrl
        };
    }
}