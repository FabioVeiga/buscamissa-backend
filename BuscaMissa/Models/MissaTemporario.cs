using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.Enums;

namespace BuscaMissa.Models
{
    public class MissaTemporaria
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DiaDaSemanaEnum DiaSemana { get; set; }
        [Required]
        public TimeSpan Horario { get; set; }
        public int IgrejaId { get; set; }
        public Igreja Igreja { get; set; } = null!;

        public static explicit operator MissaTemporaria(MissaRequest request)
        {
            return new MissaTemporaria
            {
                DiaSemana = request.DiaDaSemana,
                Horario = request.HorarioMissa,
                IgrejaId = request.IgrejaId
            };
        }
    }

}