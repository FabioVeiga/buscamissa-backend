using System.Text.Json;
using BuscaMissa.Models;

namespace BuscaMissa.Services;

/// <summary>
/// Geocodifica endereços usando Nominatim (OpenStreetMap) — gratuito, sem chave de API.
/// Política de uso: máx. 1 req/segundo, User-Agent obrigatório.
/// </summary>
public class GeocodingService(HttpClient httpClient, ILogger<GeocodingService> logger)
{
    private const string UserAgent = "BuscaMissa/1.0 (contato@buscamissa.com.br)";

    /// <summary>
    /// Tenta geocodificar o endereço da igreja e preenche Latitude/Longitude.
    /// Falha silenciosamente — coordenadas são melhoria, não bloqueante.
    /// </summary>
    public async Task GeocodeAsync(Endereco endereco)
    {
        try
        {
            var coords = await BuscarCoordenadasAsync(endereco);

            if (coords is null)
            {
                logger.LogWarning(
                    "Geocoding sem resultado para Id={Id}: {Logradouro}, {Numero}, {Bairro}, {Localidade}/{Uf}, CEP {Cep}",
                    endereco.Id, endereco.Logradouro, endereco.Numero, endereco.Bairro,
                    endereco.Localidade, endereco.Uf, endereco.Cep);
                return;
            }

            endereco.Latitude = (decimal)coords.Value.Lat;
            endereco.Longitude = (decimal)coords.Value.Lon;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Geocoding falhou para {Logradouro}, {Localidade}/{Uf}",
                endereco.Logradouro, endereco.Localidade, endereco.Uf);
        }
    }

    // ── privado ───────────────────────────────────────────────────────────────

    private async Task<(double Lat, double Lon)?> BuscarCoordenadasAsync(Endereco e)
    {
        var cidade = e.Localidade;
        var uf = e.Uf;

        // Cascata do mais preciso (rua+número) ao mais grosseiro (centro da cidade).
        // A primeira que retornar vence. Centro de cidade é aproximado, mas
        // suficiente para "missas próximas" e melhor do que coordenada nula.
        var tentativas = new List<string?>
        {
            e.Numero > 0 ? $"{e.Logradouro}, {e.Numero}, {cidade}, {uf}, Brasil" : null,
            $"{e.Logradouro}, {cidade}, {uf}, Brasil",
            !string.IsNullOrWhiteSpace(e.Bairro) ? $"{e.Bairro}, {cidade}, {uf}, Brasil" : null,
            !string.IsNullOrWhiteSpace(e.Cep) ? $"{e.Cep.Replace("-", "")}, Brasil" : null,
            $"{cidade}, {uf}, Brasil",
        };

        foreach (var query in tentativas)
        {
            if (string.IsNullOrWhiteSpace(query))
                continue;

            var resultado = await ConsultarNominatimAsync(query);
            if (resultado is not null)
                return resultado;
        }

        return null;
    }

    private async Task<(double Lat, double Lon)?> ConsultarNominatimAsync(string query)
    {
        var url = $"https://nominatim.openstreetmap.org/search" +
                  $"?q={Uri.EscapeDataString(query)}" +
                  $"&format=json&limit=1&countrycodes=br";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", UserAgent);

        using var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
            return null;

        var primeiro = root[0];
        if (!primeiro.TryGetProperty("lat", out var latEl) ||
            !primeiro.TryGetProperty("lon", out var lonEl))
            return null;

        if (!double.TryParse(latEl.GetString(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var lat))
            return null;

        if (!double.TryParse(lonEl.GetString(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var lon))
            return null;

        return (lat, lon);
    }
}
