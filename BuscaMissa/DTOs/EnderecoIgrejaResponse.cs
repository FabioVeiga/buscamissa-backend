using BuscaMissa.Models;

namespace BuscaMissa.DTOs
{
    public class EnderecoIgrejaResponse
    {
        public int Id { get; set; }
        public int Cep { get; set; } = default!;
        public string Logradouro { get; set; } = default!;
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = default!;
        public string Localidade { get; set; } = default!;
        public string Uf { get; set; } = default!;
        public string Estado { get; set; } = default!;
        public string Regiao { get; set; } = default!;
        public int IgrejaId { get; set; }


        public static explicit operator EnderecoIgrejaResponse(Endereco endereco)
        {
            return new EnderecoIgrejaResponse{
                Id = endereco.Id,
                Cep = 1,
                Logradouro = endereco.Logradouro,
                Complemento = endereco.Complemento,
                Bairro = endereco.Bairro,
                Localidade = endereco.Localidade,
                Uf = endereco.Uf,
                Estado = endereco.Estado,
                Regiao = endereco.Regiao,
                IgrejaId = endereco.IgrejaId
            };
        }
    }
}