using System.Text.Json;
using BuscaMissa.DTOs.EnderecoDto;

namespace BuscaMissa.Services.v1
{
    public class ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger)
    {
        public async Task<EnderecoViaCepResponse?> ConsultarCepAsync(string cep)
        {
            var apis = new[]
            {
                new { Name = "ViaCEP", Url = $"https://viacep.com.br/ws/{cep}/json/" },
                new { Name = "BrasilAPI", Url = $"https://brasilapi.com.br/api/cep/v1/{cep}" },
                new { Name = "AwesomeAPI", Url = $"https://cep.awesomeapi.com.br/json/{cep}" }
            };

            // Defina o timeout do httpClient apenas uma vez, se necessário
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            foreach (var api in apis)
            {
                try
                {
                    logger.LogInformation($"Consultando CEP {cep} na API {api.Name}");
                    var response = await httpClient.GetFromJsonAsync<JsonDocument>(api.Url);

                    if (response != null)
                    {
                        var result = MapResponse(api.Name, response);
                        if (result != null && !result.Erro)
                        {
                            logger.LogInformation($"CEP encontrado na API {api.Name}");
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Falha ao consultar CEP na API {api.Name}");
                }
            }
            return null;
        }
        
        /// <summary>
        /// Busca possíveis CEPs candidatos a partir de UF, cidade e endereço/logradouro via ViaCEP.
        /// Exemplo:
        /// https://viacep.com.br/ws/SP/S%C3%A3o%20Paulo/Rua%20Ernesto%20Boscariol/json/
        /// </summary>
        public async Task<IEnumerable<EnderecoViaCepResponse>> ConsultarCepPorEnderecoAsync(
            string uf,
            string cidade,
            string endereco,
            string? bairro = null)
        {
            var ufTratada = uf.Trim().ToUpperInvariant();
            var cidadeTratada = cidade.Trim();
            var enderecoTratado = endereco.Trim();

            var ufEncoded = Uri.EscapeDataString(ufTratada);
            var cidadeEncoded = Uri.EscapeDataString(cidadeTratada);
            var enderecoEncoded = Uri.EscapeDataString(enderecoTratado);

            var url = $"https://viacep.com.br/ws/{ufEncoded}/{cidadeEncoded}/{enderecoEncoded}/json/";

            logger.LogInformation(
                "Buscando candidatos de endereço no ViaCEP — UF: {Uf}, Cidade: {Cidade}, Endereco: {Endereco}, Bairro: {Bairro}",
                ufTratada,
                cidadeTratada,
                enderecoTratado,
                bairro ?? "não informado");

            try
            {
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var response = await httpClient.GetFromJsonAsync<List<EnderecoViaCepResponse>>(url);

                if (response == null || response.Count == 0)
                {
                    logger.LogWarning(
                        "Nenhum candidato encontrado no ViaCEP para UF: {Uf}, Cidade: {Cidade}, Endereco: {Endereco}",
                        ufTratada,
                        cidadeTratada,
                        enderecoTratado);

                    return Enumerable.Empty<EnderecoViaCepResponse>();
                }

                var candidatos = response
                    .Where(r => !r.Erro)
                    .Select(r =>
                    {
                        r.Estado = string.IsNullOrWhiteSpace(r.Estado)
                            ? GetEstado(r.Uf)
                            : r.Estado;

                        r.Regiao = string.IsNullOrWhiteSpace(r.Regiao)
                            ? GetRegiao(r.Uf)
                            : r.Regiao;

                        return r;
                    });

                if (!string.IsNullOrWhiteSpace(bairro))
                {
                    var bairroTratado = bairro.Trim();

                    candidatos = candidatos.Where(r =>
                        !string.IsNullOrWhiteSpace(r.Bairro) &&
                        r.Bairro.Contains(bairroTratado, StringComparison.OrdinalIgnoreCase));
                }

                return candidatos.ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Erro ao buscar candidatos de endereço no ViaCEP — UF: {Uf}, Cidade: {Cidade}, Endereco: {Endereco}",
                    ufTratada,
                    cidadeTratada,
                    enderecoTratado);

                return Enumerable.Empty<EnderecoViaCepResponse>();
            }
        }

        // ─── Mapper específico para a busca reversa (ViaCEP sempre retorna lista) ──
        private EnderecoViaCepResponse? MapResponseReverso(JsonDocument response)
        {
            try
            {
                var root = response.RootElement;

                return new EnderecoViaCepResponse
                {
                    Cep         = root.GetProperty("cep").GetString() ?? string.Empty,
                    Logradouro  = root.GetProperty("logradouro").GetString() ?? string.Empty,
                    Bairro      = root.GetProperty("bairro").GetString() ?? string.Empty,
                    Localidade  = root.GetProperty("localidade").GetString() ?? string.Empty,
                    Uf          = root.GetProperty("uf").GetString() ?? string.Empty,
                    Estado      = GetEstado(root.GetProperty("uf").GetString() ?? string.Empty),
                    Regiao      = GetRegiao(root.GetProperty("uf").GetString() ?? string.Empty),
                    Erro        = root.TryGetProperty("erro", out _)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao mapear item da busca reversa.");
                return null;
            }
        }
        
        private EnderecoViaCepResponse? MapResponse(string apiName, JsonDocument response)
        {
            try
            {
                var root = response.RootElement;

                return apiName switch
                {
                    "ViaCEP" => new EnderecoViaCepResponse
                    {
                        Cep = root.GetProperty("cep").GetString() ?? string.Empty,
                        Logradouro = root.GetProperty("logradouro").GetString() ?? string.Empty,
                        Bairro = root.GetProperty("bairro").GetString() ?? string.Empty,
                        Localidade = root.GetProperty("localidade").GetString() ?? string.Empty,
                        Uf = root.GetProperty("uf").GetString() ?? string.Empty,
                        Estado = root.GetProperty("estado").GetString() ?? string.Empty,
                        Regiao = GetRegiao(root.GetProperty("uf").GetString() ?? string.Empty),
                        Erro = root.TryGetProperty("erro", out _)
                    },
                    "BrasilAPI" => new EnderecoViaCepResponse
                    {
                        Cep = root.GetProperty("cep").GetString() ?? string.Empty,
                        Logradouro = root.GetProperty("street").GetString() ?? string.Empty,
                        Bairro = root.GetProperty("neighborhood").GetString() ?? string.Empty,
                        Localidade = root.GetProperty("city").GetString() ?? string.Empty,
                        Uf = root.GetProperty("state").GetString() ?? string.Empty,
                        Estado = GetEstado(root.GetProperty("state").GetString() ?? string.Empty),
                        Regiao = GetRegiao(root.GetProperty("state").GetString() ?? string.Empty),
                        Erro = false
                    },
                    "AwesomeAPI" => new EnderecoViaCepResponse
                    {
                        Cep = root.GetProperty("cep").GetString() ?? string.Empty,
                        Logradouro = root.GetProperty("address").GetString() ?? string.Empty,
                        Bairro = root.GetProperty("district").GetString() ?? string.Empty,
                        Localidade = root.GetProperty("city").GetString() ?? string.Empty,
                        Uf = root.GetProperty("state").GetString() ?? string.Empty,
                        Estado = GetEstado(root.GetProperty("state").GetString() ?? string.Empty),
                        Regiao = GetRegiao(root.GetProperty("state").GetString() ?? string.Empty),
                        Erro = false
                    },
                    _ => null
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao mapear resposta da API {ApiName}", apiName);
                return null;
            }
        }

        private static string GetEstado(string uf)
        {
            var estados = new Dictionary<string, string>
            {
                ["AC"] = "Acre", 
                ["AL"] = "Alagoas", 
                ["AP"] = "Amapá", 
                ["AM"] = "Amazonas", 
                ["BA"] = "Bahia", 
                ["CE"] = "Ceará", 
                ["DF"] = "Distrito Federal", 
                ["ES"] = "Espírito Santo", 
                ["GO"] = "Goiás", 
                ["MA"] = "Maranhão", 
                ["MT"] = "Mato Grosso", 
                ["MS"] = "Mato Grosso do Sul", 
                ["MG"] = "Minas Gerais", 
                ["PA"] = "Pará", 
                ["PB"] = "Paraíba", 
                ["PR"] = "Paraná", 
                ["PE"] = "Pernambuco", 
                ["PI"] = "Piauí", 
                ["RJ"] = "Rio de Janeiro", 
                ["RN"] = "Rio Grande do Norte", 
                ["RS"] = "Rio Grande do Sul", 
                ["RO"] = "Rondônia", 
                ["RR"] = "Roraima", 
                ["SC"] = "Santa Catarina", 
                ["SP"] = "São Paulo", 
                ["SE"] = "Sergipe", 
                ["TO"] = "Tocantins"
            };

            return estados.TryGetValue(uf, out var estado) ? estado : "Desconhecido";
        }
        
        private static string GetRegiao(string uf)
        {
            var regioes = new Dictionary<string, string>
            {
                ["AC"] = "Norte",
                ["AP"] = "Norte",
                ["AM"] = "Norte",
                ["PA"] = "Norte",
                ["RO"] = "Norte",
                ["RR"] = "Norte",
                ["TO"] = "Norte",
                ["AL"] = "Nordeste",
                ["BA"] = "Nordeste",
                ["CE"] = "Nordeste",
                ["MA"] = "Nordeste",
                ["PB"] = "Nordeste",
                ["PE"] = "Nordeste",
                ["PI"] = "Nordeste",
                ["RN"] = "Nordeste",
                ["SE"] = "Nordeste",
                ["DF"] = "Centro-Oeste",
                ["GO"] = "Centro-Oeste",
                ["MT"] = "Centro-Oeste",
                ["MS"] = "Centro-Oeste",
                ["ES"] = "Sudeste",
                ["MG"] = "Sudeste",
                ["RJ"] = "Sudeste",
                ["SP"] = "Sudeste",
                ["PR"] = "Sul",
                ["RS"] = "Sul",
                ["SC"] = "Sul"
            };

            return regioes.TryGetValue(uf, out var regiao) ? regiao : "Desconhecida";
        }
    }

}



