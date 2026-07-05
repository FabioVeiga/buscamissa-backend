namespace BuscaMissa.DTOs.MetricasDto;

// Payload único da tela de Indicadores (Etapa 8): evita múltiplas chamadas do frontend.
public record IndicadoresResponse(
    TotaisSistemaResponse Cards,
    RankingsResponse Rankings,
    PeriodoResponse Periodo,
    DateTime DataConsulta);
