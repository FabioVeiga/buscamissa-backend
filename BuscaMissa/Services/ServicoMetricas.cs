using BuscaMissa.Enums;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using BuscaMissa.Repositorios;

namespace BuscaMissa.Services;

public class ServicoMetricas(
    IMetricaDiariaRepositorio repositorio,
    ILogger<ServicoMetricas> logger)
{
    public async Task IncrementarAsync(
        TipoEntidadeMetricaEnum tipoEntidade,
        int entidadeId,
        TipoMetricaEnum tipoMetrica)
    {
        try
        {
            var hoje = DataHoraHelper.HojeBrasil();

            var metrica = await repositorio.ObterAsync(tipoEntidade, entidadeId, tipoMetrica, hoje);

            if (metrica is null)
            {
                metrica = new MetricaDiaria
                {
                    TipoEntidade = tipoEntidade,
                    EntidadeId   = entidadeId,
                    TipoMetrica  = tipoMetrica,
                    Data         = hoje,
                    Quantidade   = 1,
                    CriadoEm    = DateTime.UtcNow,
                    AtualizadoEm = DateTime.UtcNow,
                };

                await repositorio.CriarAsync(metrica);
                return;
            }

            metrica.Quantidade++;
            metrica.AtualizadoEm = DateTime.UtcNow;
            await repositorio.SalvarAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Erro ao incrementar métrica {TipoMetrica} para {TipoEntidade} {EntidadeId}",
                tipoMetrica, tipoEntidade, entidadeId);
            throw;
        }
    }
}
