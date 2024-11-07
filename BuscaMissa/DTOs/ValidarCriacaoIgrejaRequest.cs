using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs
{
    public class ValidarCriacaoIgrejaRequest
    {
        [Required]
        public int CodigoToken { get; set; }
        [Required]
        public int IgrejaId { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Nome { get; set; } = null!;
    }
}