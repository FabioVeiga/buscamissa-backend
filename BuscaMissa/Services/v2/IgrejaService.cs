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
                    Endereco = (EnderecoIgrejaResponse)x.Endereco,   
                    Usuario = x.Usuario == null ? null : (UsuarioDtoResponse)x.Usuario,
                    Alteracao = x.Alteracao,
                    Ativo = x.Ativo,
                    Criacao = x.Criacao,
                    ImagemUrl = x.ImagemUrl == null ? null: imagemService.ObterUrlAzureBlob($"igreja/{x.ImagemUrl!}"),
                    Paroco = x.Paroco,
                    Missas = x.Missas.Select(m => (MissaResponse)m).ToList(),
                    Contato = x.Contato == null ? null : (IgrejaContatoResponse)x.Contato,
                    RedesSociais = x.RedesSociais == null ? Array.Empty<IgrejaRedesSociaisResponse>() : x.RedesSociais.Select(r => (IgrejaRedesSociaisResponse)r).ToList(),
                    Denuncia = x.Denuncia == null ? null : string.IsNullOrEmpty(x.Denuncia.AcaoRealizada) ? (DenunciarIgrejaAdminResponse)x.Denuncia : null
                });

                var resultado = await aux.PaginacaoAsync(filtro.Paginacao.PageIndex, filtro.Paginacao.PageSize);
                return resultado;
            }
            catch (Exception)
            {

                throw;
            }
        } 
    
}