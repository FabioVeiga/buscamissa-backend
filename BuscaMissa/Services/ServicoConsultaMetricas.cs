using BuscaMissa.Context;
using BuscaMissa.DTOs.MetricasDto;
using BuscaMissa.Enums;
using BuscaMissa.Helpers;
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
        var dataInicio = DataHoraHelper.HojeBrasil().AddDays(-30);

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

    public async Task<IList<RankingItemResponse>> ObterRankingMaisVisualizadasAsync(
        int top = 10, DateOnly? dataInicio = null, DateOnly? dataFim = null)
        => await ObterRankingAsync(TipoMetricaEnum.VisualizacaoIgreja, top, dataInicio, dataFim);

    public async Task<IList<RankingItemResponse>> ObterRankingMaisFavoritadasAsync(
        int top = 10, DateOnly? dataInicio = null, DateOnly? dataFim = null)
        => await ObterRankingAsync(TipoMetricaEnum.Favorito, top, dataInicio, dataFim);

    public async Task<IList<RankingItemResponse>> ObterRankingMaisCompartilhadasAsync(
        int top = 10, DateOnly? dataInicio = null, DateOnly? dataFim = null)
        => await ObterRankingAsync(TipoMetricaEnum.Compartilhamento, top, dataInicio, dataFim);

    public async Task<IList<RankingItemResponse>> ObterRankingMaisRotasAbertasAsync(
        int top = 10, DateOnly? dataInicio = null, DateOnly? dataFim = null)
        => await ObterRankingAsync(TipoMetricaEnum.CliqueRota, top, dataInicio, dataFim);

    // Sem DataInicial/DataFinal informados, considera todo o histórico (sem filtro de data).
    private async Task<IList<RankingItemResponse>> ObterRankingAsync(
        TipoMetricaEnum tipoMetrica, int top, DateOnly? dataInicio, DateOnly? dataFim)
    {
        var query = context.MetricasDiarias
            .AsNoTracking()
            .Where(x =>
                x.TipoEntidade == TipoEntidadeMetricaEnum.Igreja &&
                x.TipoMetrica == tipoMetrica);

        if (dataInicio is not null)
            query = query.Where(x => x.Data >= dataInicio);

        if (dataFim is not null)
            query = query.Where(x => x.Data <= dataFim);

        // Projeção com tipo anônimo para compatibilidade com EF Core/MySQL:
        // records com construtor parametrizado não são suportados na tradução SQL.
        var rows = await query
            .GroupBy(x => x.EntidadeId)
            .Select(g => new { EntidadeId = g.Key, Quantidade = g.Sum(x => x.Quantidade) })
            .OrderByDescending(x => x.Quantidade)
            .Take(top)
            .ToListAsync();

        return rows.Select(x => new RankingItemResponse(x.EntidadeId, x.Quantidade)).ToList();
    }

    /// <summary>Resolve o ranking (Id + Quantidade) para o formato exibível no dashboard, com nome/cidade/UF da igreja.</summary>
    private async Task<IList<RankingIgrejaResponse>> ObterRankingIgrejasAsync(
        TipoMetricaEnum tipoMetrica, int top, DateOnly? dataInicio, DateOnly? dataFim)
    {
        var ranking = await ObterRankingAsync(tipoMetrica, top, dataInicio, dataFim);
        if (ranking.Count == 0) return new List<RankingIgrejaResponse>();

        var ids = ranking.Select(x => x.EntidadeId).ToList();
        var igrejas = await context.Igrejas
            .Include(x => x.Endereco)
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => new { x.Nome, Cidade = x.Endereco.Localidade, x.Endereco.Uf });

        // Ordenação: quantidade decrescente; em empate, por nome (regra da Etapa 2).
        return ranking
            .Select(x =>
            {
                var dados = igrejas.GetValueOrDefault(x.EntidadeId);
                return new RankingIgrejaResponse(
                    x.EntidadeId,
                    dados?.Nome ?? "—",
                    dados?.Cidade ?? "—",
                    dados?.Uf ?? "—",
                    x.Quantidade);
            })
            .OrderByDescending(x => x.Quantidade)
            .ThenBy(x => x.Nome)
            .ToList();
    }

    /// <summary>Totais gerais do sistema (todas as igrejas). Sem datas informadas, considera todo o histórico.</summary>
    public async Task<TotaisSistemaResponse> ObterTotaisSistemaAsync(DateOnly? dataInicio = null, DateOnly? dataFim = null)
    {
        var query = context.MetricasDiarias
            .AsNoTracking()
            .Where(x => x.TipoEntidade == TipoEntidadeMetricaEnum.Igreja);

        if (dataInicio is not null)
            query = query.Where(x => x.Data >= dataInicio);

        if (dataFim is not null)
            query = query.Where(x => x.Data <= dataFim);

        var totais = await query
            .GroupBy(x => x.TipoMetrica)
            .Select(g => new { Tipo = g.Key, Total = g.Sum(x => x.Quantidade) })
            .ToListAsync();

        int Obter(TipoMetricaEnum tipo) => totais.FirstOrDefault(x => x.Tipo == tipo)?.Total ?? 0;

        return new TotaisSistemaResponse(
            Obter(TipoMetricaEnum.VisualizacaoIgreja),
            Obter(TipoMetricaEnum.Favorito),
            Obter(TipoMetricaEnum.Compartilhamento));
    }

    /// <summary>
    /// Payload único da tela de Indicadores (Cards + Rankings + Período + DataConsulta) — Etapa 8:
    /// substitui múltiplas chamadas por uma única consulta ao backend.
    /// DataInicial/DataFinal são opcionais — se ausentes, considera todo o histórico (Etapa 1).
    /// </summary>
    public async Task<IndicadoresResponse> ObterIndicadoresAsync(
        int top = 10, DateOnly? dataInicial = null, DateOnly? dataFinal = null)
    {
        var cards = await ObterTotaisSistemaAsync(dataInicial, dataFinal);
        var maisVisualizadas = await ObterRankingIgrejasAsync(TipoMetricaEnum.VisualizacaoIgreja, top, dataInicial, dataFinal);
        var maisFavoritadas = await ObterRankingIgrejasAsync(TipoMetricaEnum.Favorito, top, dataInicial, dataFinal);
        var maisCompartilhadas = await ObterRankingIgrejasAsync(TipoMetricaEnum.Compartilhamento, top, dataInicial, dataFinal);
        var maisRotasAbertas = await ObterRankingIgrejasAsync(TipoMetricaEnum.CliqueRota, top, dataInicial, dataFinal);

        var rankings = new RankingsResponse(maisVisualizadas, maisFavoritadas, maisCompartilhadas, maisRotasAbertas);
        var periodo = new PeriodoResponse(dataInicial, dataFinal);

        return new IndicadoresResponse(cards, rankings, periodo, DateTime.UtcNow);
    }
}
