using BuscaMissa.Context;
using BuscaMissa.Enums;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Repositorios;

public class MetricaDiariaRepositorio(ApplicationDbContext context) : IMetricaDiariaRepositorio
{
    public async Task<MetricaDiaria?> ObterAsync(
        TipoEntidadeMetricaEnum tipoEntidade,
        int entidadeId,
        TipoMetricaEnum tipoMetrica,
        DateOnly data)
    {
        return await context.MetricasDiarias
            .FirstOrDefaultAsync(x =>
                x.TipoEntidade == tipoEntidade &&
                x.EntidadeId   == entidadeId   &&
                x.TipoMetrica  == tipoMetrica  &&
                x.Data         == data);
    }

    public async Task<MetricaDiaria> CriarAsync(MetricaDiaria metrica)
    {
        context.MetricasDiarias.Add(metrica);
        await context.SaveChangesAsync();
        return metrica;
    }

    public async Task SalvarAsync()
    {
        await context.SaveChangesAsync();
    }
}
