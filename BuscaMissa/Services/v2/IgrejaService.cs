using BuscaMissa.Context;
using BuscaMissa.DTOs.v2.IgrejaDto;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services.v2;

public class IgrejaService(
    ApplicationDbContext context,
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
}