namespace BuscaMissa.DTOs.MetricasDto;

public record RankingsResponse(
    IList<RankingIgrejaResponse> MaisVisualizadas,
    IList<RankingIgrejaResponse> MaisFavoritadas,
    IList<RankingIgrejaResponse> MaisCompartilhadas,
    IList<RankingIgrejaResponse> MaisRotasAbertas);
