using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs
{
    public class IgrejaRequest
    {
        [Required]
        public string Nome { get; set; } = default!;
        [Required]
        [EmailAddress]
        public string EmailUsuario { get; set; } = default!;
        [Required(ErrorMessage = "{0} é obrigatório!")]
        public string NomeUsuario { get; set; } = default!;
        public string? Paroco { get; set; }
        public string? Imagem { get; set; }
        public ICollection<MissaRequest> Missas { get; set; } = [];
        public EnderecoIgrejaRequest Endereco { get; set; } = default!;
    }
}