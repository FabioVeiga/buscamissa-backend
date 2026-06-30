namespace BuscaMissa.DTOs.MetricasDto;

/// <summary>
/// DTO consolidado com os indicadores de uma igreja, pensado para uso futuro
/// no painel próprio da igreja. Ainda sem service ou endpoint associado.
/// </summary>
public record PainelMetricasIgrejaResponse(
    int Visualizacoes,
    int Favoritos,
    int Rotas,
    int Telefone,
    int Instagram,
    int Compartilhamentos,
    int Sugestoes);
