using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.Helpers;
namespace BuscaMissa.Models
{
    public class Endereco
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Cep { get; set; } = default!;
        [Required]
        public string Logradouro { get; set; } = default!;
        public string? Complemento { get; set; }
        [Required]
        public string Bairro { get; set; } = default!;
        [Required]
        public string Localidade { get; set; } = default!;
        [Required]
        public string Uf { get; set; } = default!;
        [Required]
        public string Estado { get; set; } = default!;
        [Required]
        public string Regiao { get; set; } = default!;
        [Required]
        public int Numero { get; set; }

        public string? Latitude { get; set; }
        public string? Longitude { get; set; }

        public int IgrejaId { get; set; }
        public Igreja Igreja { get; set; } = null!;
        
        public static explicit operator Endereco(EnderecoIgrejaRequest request)
        {
            
            return new Endereco{
                Bairro = request.Bairro,
                Cep = CepHelper.FormatarCep(request.Cep),
                Complemento = request.Complemento,
                Estado = request.Estado,
                Localidade = request.Localidade,
                Logradouro = request.Logradouro,
                Regiao = request.Regiao,
                Uf = request.Uf,
                Numero = request.Numero,
            };
        }
    }

}