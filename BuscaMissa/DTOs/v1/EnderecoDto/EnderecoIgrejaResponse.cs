using BuscaMissa.Models;

namespace BuscaMissa.DTOs.EnderecoDto
{
    public class EnderecoIgrejaResponse
    {
        public int Id { get; set; }
        public string Cep { get; set; } = default!;
        public string Logradouro { get; set; } = default!;
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = default!;
        public string Localidade { get; set; } = default!;
        public string? CidadeSlug { get; set; }
        public string Uf { get; set; } = default!;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Estado { get; set; } = default!;
        public string Regiao { get; set; } = default!;
        public int Numero { get; set; }
        public int IgrejaId { get; set; }


        public static explicit operator EnderecoIgrejaResponse(Endereco endereco)
        {
            if (endereco is null) return new EnderecoIgrejaResponse();
            return new EnderecoIgrejaResponse{
                Id = endereco.Id,
                Cep = endereco.Cep,
                Logradouro = endereco.Logradouro,
                Complemento = endereco.Complemento,
                Bairro = endereco.Bairro,
                Localidade = endereco.Localidade,
                CidadeSlug = endereco.CidadeSlug,
                Uf = endereco.Uf,
                Latitude = endereco.Latitude,
                Longitude = endereco.Longitude,
                Estado = endereco.Estado,
                Regiao = endereco.Regiao,
                Numero = endereco.Numero,
                IgrejaId = endereco.IgrejaId
            };
        }
    }
}