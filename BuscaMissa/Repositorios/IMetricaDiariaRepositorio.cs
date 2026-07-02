using BuscaMissa.Enums;
using BuscaMissa.Models;

namespace BuscaMissa.Repositorios;

public interface IMetricaDiariaRepositorio
{
    Task<MetricaDiaria?> ObterAsync(
        TipoEntidadeMetricaEnum tipoEntidade,
        int entidadeId,
        TipoMetricaEnum tipoMetrica,
        DateOnly data);

    Task<MetricaDiaria> CriarAsync(MetricaDiaria metrica);

    Task SalvarAsync();
}
