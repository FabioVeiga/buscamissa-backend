using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs.IgrejaDto;

namespace BuscaMissa.Models
{
    public class IgrejaReportarProblema
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Descricao { get; set; } = default!;
        public string? AcaoRealizada { get; set; }
        [Required]
        public string Nome { get; set; } = default!;
        [Required]
        public string Email { get; set; } = default!;
        public int IgrejaId { get; set; }
        public Igreja Igreja { get; set; } = null!;

        public static explicit operator  IgrejaReportarProblema(ReportarProblemaRequest request){
            return new IgrejaReportarProblema{
                Descricao = request.Descricao,
                Nome = request.Nome,
                Email = request.Email,
                IgrejaId = request.IgrejaId
            };
        }
    }
}