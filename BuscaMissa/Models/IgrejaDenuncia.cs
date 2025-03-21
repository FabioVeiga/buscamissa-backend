using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs.IgrejaDto;

namespace BuscaMissa.Models
{
    public class IgrejaDenuncia
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Titulo { get; set; } = default!;
        [Required]
        public string Descricao { get; set; } = default!;
        public string? AcaoRealizada { get; set; }
        [Required]
        public string NomeDenunciador { get; set; } = default!;
        [Required]
        public string EmailDenunciador { get; set; } = default!;
        public int IgrejaId { get; set; }
        public Igreja Igreja { get; set; } = null!;

        public static explicit operator  IgrejaDenuncia(DenunciarIgrejaRequest request){
            return new IgrejaDenuncia{
                Titulo = request.Titulo,
                Descricao = request.Descricao,
                NomeDenunciador = request.NomeDenunciador,
                EmailDenunciador = request.EmailDenunciador,
                IgrejaId = request.IgrejaId
            };
        }
    }
}