using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class ReportarProblemaRequest
    {
        [Required]
        public string Descricao { get; set; } = default!;
        [Required]
        public string Nome { get; set; } = default!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;
        internal int IgrejaId { get; set; }
    }
}