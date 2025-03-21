using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class DenunciarIgrejaRequest
    {
        [Required]
        public string Titulo { get; set; } = default!;
        [Required]
        public string Descricao { get; set; } = default!;
        [Required]
        public string NomeDenunciador { get; set; } = default!;
        [Required]
        [EmailAddress]
        public string EmailDenunciador { get; set; } = default!;
        internal int IgrejaId { get; set; }
    }
}