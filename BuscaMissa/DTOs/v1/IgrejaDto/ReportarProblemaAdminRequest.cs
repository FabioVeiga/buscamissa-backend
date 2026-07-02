using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class ReportarProblemaAdminRequest
    {
        [Required]
        public string Solucao { get; set; } = default!;
        [Required]
        public bool EnviarEmail { get; set; }
    }
}