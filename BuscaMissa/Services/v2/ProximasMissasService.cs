using BuscaMissa.Context;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.DTOs.v2.IgrejaDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using BuscaMissa.Services.v1;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services.v2;

public class ProximasMissasService(
    ApplicationDbContext context,
    ImagemService imagemService,
    ILogger<ProximasMissasService> logger)
{
    // Fuso horário padrão do Brasil — Windows e Linux têm IDs diferentes
    private static readonly TimeZoneInfo FusoBrasilia = CarregarFusoBrasilia();

    private static TimeZoneInfo CarregarFusoBrasilia()
    {
        foreach (var id in new[] { "E. South America Standard Time", "America/Sao_Paulo" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch { /* tenta o próximo */ }
        }
        return TimeZoneInfo.Utc; // fallback improvável — UTC-0 melhor do que exceção
    }

    public async Task<List<ProximaMissaDto>> BuscarAsync(ProximasMissasRequest request)
    {
        try
        {
            var agora = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, FusoBrasilia);
            var janelaDe = agora;
            var janelaAte = agora.AddHours(request.Horas);

            var igrejas = await context.Igrejas
                .Include(x => x.Endereco)
                .Include(x => x.Missas)
                .AsNoTracking()
                .Where(x =>
                    x.Ativo &&
                    x.Endereco.Latitude != null &&
                    x.Endereco.Longitude != null)
                .ToListAsync();

            var resultado = new List<ProximaMissaDto>();

            foreach (var igreja in igrejas)
            {
                var distancia = GeoHelper.DistanciaKm(
                    (double)request.Lat,
                    (double)request.Lng,
                    (double)igreja.Endereco.Latitude!,
                    (double)igreja.Endereco.Longitude!);

                if (distancia > (double)request.RaioKm)
                    continue;

                var candidata = EncontrarProximaMissa(igreja.Missas, janelaDe, janelaAte);
                if (candidata is null)
                    continue;

                var missaResponse = (MissaResponse)candidata.Missa;
                ConfiancaCalculator.PreencherConfianca(missaResponse, igreja.Alteracao);

                resultado.Add(new ProximaMissaDto
                {
                    IgrejaId = igreja.Id,
                    Nome = igreja.Nome,
                    Slug = igreja.Slug ?? igreja.NomeUnico ?? string.Empty,
                    Uf = igreja.Endereco.Uf,
                    CidadeSlug = igreja.Endereco.CidadeSlug ?? string.Empty,
                    Bairro = igreja.Endereco.Bairro,
                    ImagemUrl = igreja.ImagemUrl is null
                        ? null
                        : imagemService.ObterUrlAzureBlob($"igreja/{igreja.ImagemUrl}"),
                    Latitude = igreja.Endereco.Latitude,
                    Longitude = igreja.Endereco.Longitude,
                    Missa = missaResponse,
                    MinutosParaInicio = candidata.MinutosParaInicio,
                    DistanciaKm = Math.Round(distancia, 2),
                });
            }

            return resultado
                .OrderBy(x => x.MinutosParaInicio)
                .ThenBy(x => x.DistanciaKm)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar próximas missas para lat={Lat} lng={Lng}", request.Lat, request.Lng);
            throw;
        }
    }

    // ── helpers privados ──────────────────────────────────────────────────────

    private record MissaCandidata(Missa Missa, int MinutosParaInicio);

    private static MissaCandidata? EncontrarProximaMissa(
        IEnumerable<Missa> missas,
        DateTime janelaDe,
        DateTime janelaAte)
    {
        MissaCandidata? melhor = null;

        foreach (var missa in missas)
        {
            var proxima = ProximaOcorrencia(missa, janelaDe);
            if (proxima > janelaAte)
                continue;

            var minutos = (int)Math.Round((proxima - janelaDe).TotalMinutes);
            if (minutos < 0)
                continue;

            if (melhor is null || minutos < melhor.MinutosParaInicio)
                melhor = new MissaCandidata(missa, minutos);
        }

        return melhor;
    }

    private static DateTime ProximaOcorrencia(Missa missa, DateTime referencia)
    {
        // DayOfWeek do C# e DiaDaSemanaEnum usam a mesma convenção: 0=Domingo
        var diaSemanaAlvo = (int)missa.DiaSemana;
        var diaAtual = (int)referencia.DayOfWeek;

        var diasAte = ((diaSemanaAlvo - diaAtual) + 7) % 7;

        var candidata = referencia.Date.AddDays(diasAte).Add(missa.Horario);

        // Mesmo dia mas horário já passou → próxima semana
        if (diasAte == 0 && candidata <= referencia)
            candidata = candidata.AddDays(7);

        return candidata;
    }
}
