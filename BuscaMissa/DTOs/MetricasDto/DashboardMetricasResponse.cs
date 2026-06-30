namespace BuscaMissa.DTOs.MetricasDto;

public record DashboardMetricasResponse(
    TotaisSistemaResponse Totais,
    IList<RankingIgrejaResponse> MaisVisualizadas,
    IList<RankingIgrejaResponse> MaisFavoritadas,
    IList<RankingIgrejaResponse> MaisCompartilhadas,
    IList<RankingIgrejaResponse> MaisRotasAbertas);
