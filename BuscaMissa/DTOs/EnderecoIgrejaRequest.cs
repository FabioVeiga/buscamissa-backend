using System.ComponentModel.DataAnnotations;
namespace BuscaMissa.DTOs
{
    public class EnderecoIgrejaRequest
    {
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
    }
}