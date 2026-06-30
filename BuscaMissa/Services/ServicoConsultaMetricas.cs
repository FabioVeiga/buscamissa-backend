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

        var rows = await query
            .GroupBy(x => x.TipoMetrica)
            .Select(g => new { TipoMetrica = g.Key, Quantidade = g.Sum(x => x.Quantidade) })
            .ToListAsync();

        return rows.Select(x => new MetricaResumoResponse(x.TipoMetrica, x.Quantidade)).ToList();
    }

    /// <summary>Métricas de uma entidade nos últimos 30 dias, agrupadas por tipo.</summary>
    public async Task<IList<MetricaResumoResponse>> ObterMetricasUltimos30DiasAsync(
        TipoEntidadeMetricaEnum tipoEntidade, int entidadeId)
    {
        var dataInicio = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30);

        var rows = await context.MetricasDiarias
            .AsNoTracking()
            .Where(x =>
                x.TipoEntidade == tipoEntidade &&
                x.EntidadeId == entidadeId &&
                x.Data >= dataInicio)
            .GroupBy(x => x.TipoMetrica)
            .Select(g => new { TipoMetrica = g.Key, Quantidade = g.Sum(x => x.Quantidade) })
            .ToListAsync();

        return rows.Select(x => new MetricaResumoResponse(x.TipoMetrica, x.Quantidade)).ToList();
    }

    public async Task<IList<RankingItemResponse>> ObterRankingMaisVisualizadasAsync(int top = 10, int dias = 30)
        => await ObterRankingAsync(TipoMetricaEnum.VisualizacaoIgreja, top, dias);

    public async Task<IList<RankingItemResponse>> ObterRankingMaisFavoritadasAsync(int top = 10, int dias = 30)
        => await ObterRankingAsync(TipoMetricaEnum.Favorito, top, dias);

    public async Task<IList<RankingItemResponse>> ObterRankingMaisCompartilhadasAsync(int top = 10, int dias = 30)
        => await ObterRankingAsync(TipoMetricaEnum.Compartilhamento, top, dias);

    public async Task<IList<RankingItemResponse>> ObterRankingMaisRotasAbertasAsync(int top = 10, int dias = 30)
        => await ObterRankingAsync(TipoMetricaEnum.CliqueRota, top, dias);

    private async Task<IList<RankingItemResponse>> ObterRankingAsync(
        TipoMetricaEnum tipoMetrica, int top, int dias)
    {
        var dataInicio = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-dias);

        // Projeção com tipo anônimo para compatibilidade com EF Core/MySQL:
        // records com construtor parametrizado não são suportados na tradução SQL.
        var rows = await context.MetricasDiarias
            .AsNoTracking()
            .Where(x =>
                x.TipoEntidade == TipoEntidadeMetricaEnum.Igreja &&
                x.TipoMetrica == tipoMetrica &&
                x.Data >= dataInicio)
            .GroupBy(x => x.EntidadeId)
            .Select(g => new { EntidadeId = g.Key, Quantidade = g.Sum(x => x.Quantidade) })
            .OrderByDescending(x => x.Quantidade)
            .Take(top)
            .ToListAsync();

        return rows.Select(x => new RankingItemResponse(x.EntidadeId, x.Quantidade)).ToList();
    }

    /// <summary>Resolve o ranking (Id + Quantidade) para o formato exibível no dashboard, com o nome da igreja.</summary>
    private async Task<IList<RankingIgrejaResponse>> ObterRankingIgrejasAsync(
        TipoMetricaEnum tipoMetrica, int top, int dias)
    {
        var ranking = await ObterRankingAsync(tipoMetrica, top, dias);
        if (ranking.Count == 0) return new List<RankingIgrejaResponse>();

        var ids = ranking.Select(x => x.EntidadeId).ToList();
        var nomes = await context.Igrejas
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Nome);

        return ranking
            .Select(x => new RankingIgrejaResponse(x.EntidadeId, nomes.GetValueOrDefault(x.EntidadeId, "—"), x.Quantidade))
            .ToList();
    }

    /// <summary>Totais gerais do sistema (todas as igrejas) nos últimos 30 dias.</summary>
    public async Task<TotaisSistemaResponse> ObterTotaisSistemaUltimos30DiasAsync()
    {
        var dataInicio = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30);

        var totais = await context.MetricasDiarias
            .AsNoTracking()
            .Where(x => x.TipoEntidade == TipoEntidadeMetricaEnum.Igreja && x.Data >= dataInicio)
            .GroupBy(x => x.TipoMetrica)
            .Select(g => new { Tipo = g.Key, Total = g.Sum(x => x.Quantidade) })
            .ToListAsync();

        int Obter(TipoMetricaEnum tipo) => totais.FirstOrDefault(x => x.Tipo == tipo)?.Total ?? 0;

        return new TotaisSistemaResponse(
            Obter(TipoMetricaEnum.VisualizacaoIgreja),
            Obter(TipoMetricaEnum.Favorito),
            Obter(TipoMetricaEnum.Compartilhamento));
    }

    /// <summary>Dados completos da tela de Indicadores: totais do sistema + 4 rankings de igrejas.</summary>
    public async Task<DashboardMetricasResponse> ObterDashboardAsync(int top = 10, int dias = 30)
    {
        var totais = await ObterTotaisSistemaUltimos30DiasAsync();
        var maisVisualizadas = await ObterRankingIgrejasAsync(TipoMetricaEnum.VisualizacaoIgreja, top, dias);
        var maisFavoritadas = await ObterRankingIgrejasAsync(TipoMetricaEnum.Favorito, top, dias);
        var maisCompartilhadas = await ObterRankingIgrejasAsync(TipoMetricaEnum.Compartilhamento, top, dias);
        var maisRotasAbertas = await ObterRankingIgrejasAsync(TipoMetricaEnum.CliqueRota, top, dias);

        return new DashboardMetricasResponse(
            totais, maisVisualizadas, maisFavoritadas, maisCompartilhadas, maisRotasAbertas);
    }
}
