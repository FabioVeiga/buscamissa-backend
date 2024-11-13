using System.Text.Json.Serialization;

namespace BuscaMissa.DTOs.EnderecoDto
{
    public class EnderecoViaCepResponse
    {
        [JsonPropertyName("cep")]
        public string Cep { get; set; } = null!;

        [JsonPropertyName("logradouro")]
        public string Logradouro { get; set; }  = null!;

        [JsonPropertyName("complemento")]
        public string Complemento { get; set; } = null!;

        [JsonPropertyName("unidade")]
        public string Unidade { get; set; } = null!;

        [JsonPropertyName("bairro")]
        public string Bairro { get; set; } = null!;

        [JsonPropertyName("localidade")]
        public string Localidade { get; set; } = null!;

        [JsonPropertyName("uf")]
        public string Uf { get; set; } = null!;

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = null!;

        [JsonPropertyName("regiao")]
        public string Regiao { get; set; } = null!;

        [JsonPropertyName("ibge")]
        public string Ibge { get; set; } = null!;

        [JsonPropertyName("gia")]
        public string Gia { get; set; } = null!;

        [JsonPropertyName("ddd")]
        public string Ddd { get; set; } = null!;

        [JsonPropertyName("siafi")]
        public string Siafi { get; set; } = null!;
    }
}