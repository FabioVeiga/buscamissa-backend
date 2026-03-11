using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class DenunciarIgrejaAdminRequest
    {
        [Required]
        public string Solucao { get; set; } = default!;
        [Required]
        public bool EnviarEmailDenunciador { get; set; }
    }
}