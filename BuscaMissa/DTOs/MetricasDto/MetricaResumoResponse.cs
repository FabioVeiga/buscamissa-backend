using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.MetricasDto;

public record MetricaResumoResponse(TipoMetricaEnum TipoMetrica, int Quantidade);
