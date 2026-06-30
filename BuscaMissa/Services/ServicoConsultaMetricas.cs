using BuscaMissa.Context;
using BuscaMissa.DTOs.MetricasDto;
using BuscaMissa.Enums;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services;

public class ServicoConsultaMetricas(ApplicationDbContext context)
{
    /// <summary>Métricas de uma igreja, agrupadas por tipo. Sem datas, considera todo o histórico.</summary>
    public async Task<IList<MetricaResumoResponse>> ObterMetricasIgrejaAsync(
        int igrejaId, DateOnly? dataInicio = null, DateOnly? dataFim = null)
    {
        var query = context.MetricasDiarias
            .AsNoTracking()
            .Where(x => x.TipoEntidade == TipoEntidadeMetricaEnum.Igreja && x.EntidadeId == igrejaId);

        if (dataInicio is not null)
            query = query.Where(x => x.Data >= dataInicio);

        if (dataFim is not null)
            query = query.Where(x => x.Data <= dataFim);

        return await query
            .GroupBy(x => x.TipoMetrica)
            .Select(g => new MetricaResumoResponse(g.Key, g.Sum(x => x.Quantidade)))
            .ToListAsync();
    }

    /// <summary>Métricas de uma entidade nos últimos 30 dias, agrupadas por tipo.</summary>
    public async Task<IList<MetricaResumoResponse>> ObterMetricasUltimos30DiasAsync(
        TipoEntidadeMetricaEnum tipoEntidade, int entidadeId)
    {
        var dataInicio = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30);

        return await context.MetricasDiarias
            .AsNoTracking()
            .Where(x =>
                x.TipoEntidade == tipoEntidade &&
                x.EntidadeId == entidadeId &&
                x.Data >= dataInicio)
            .GroupBy(x => x.TipoMetrica)
            .Select(g => new MetricaResumoResponse(g.Key, g.Sum(x => x.Quantidade)))
            .ToListAsync();
    }

    public async Task<IList<RankingItemResponse>> ObterRankingMaisVisualizadasAsync(int top = 10, int dias = 30)
        => await ObterRankingAsync(TipoMetricaEnum.VisualizacaoIgreja, top, dias);

    public async Task<IList<RankingItemResponse>> ObterRankingMaisFavoritadasAsync(int top = 10, int dias = 30)
        => await ObterRankingAsync(TipoMetricaEnum.Favorito, top, dias);

    public async Task<IList<RankingItemResponse>> ObterRankingMaisCompartilhadasAsync(int top = 10, int dias = 30)
        => await ObterRankingAsync(TipoMetricaEnum.Compartilhamento, top, dias);

    private async Task<IList<RankingItemResponse>> ObterRankingAsync(
        TipoMetricaEnum tipoMetrica, int top, int dias)
    {
        var dataInicio = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-dias);

        return await context.MetricasDiarias
            .AsNoTracking()
            .Where(x =>
                x.TipoEntidade == TipoEntidadeMetricaEnum.Igreja &&
                x.TipoMetrica == tipoMetrica &&
                x.Data >= dataInicio)
            .GroupBy(x => x.EntidadeId)
            .Select(g => new RankingItemResponse(g.Key, g.Sum(x => x.Quantidade)))
            .OrderByDescending(x => x.Quantidade)
            .Take(top)
            .ToListAsync();
    }
}
