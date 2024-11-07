using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs
{
    public class CriarIgrejaRequest
    {
        [Required]
        public string EmailUsuario { get; set; } = null!;

        [Required]
        public IgrejaRequest Igreja { get; set; } = null!;
    }
}