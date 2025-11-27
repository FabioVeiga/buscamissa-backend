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
    public async Task<bool> TemNomeNomeUnicoAsync(string nomeUnico)
    {
        try
        {
            var model = await context.Igrejas.AsNoTracking().FirstOrDefaultAsync(x => x.NomeUnico == nomeUnico);
            return model != null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting InformacoesGeraisResponse");
            throw;
        }
    }
    
    public async Task<Igreja> InserirAsync(CriacaoIgrejaRequest request)
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