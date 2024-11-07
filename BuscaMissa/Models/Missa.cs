using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs;
using BuscaMissa.Enums;

namespace BuscaMissa.Models
{
    public class Missa
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DiaDaSemanaEnum DiaSemana { get; set; }
        [Required]
        public TimeSpan Horario { get; set; }
        public int IgrejaId { get; set; }
        public Igreja Igreja { get; set; } = null!;

        public static explicit operator Missa(MissaRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "MissaRequest cannot be null.");
            }

            if (!TimeSpan.TryParse(request.Horario, out var horario))
            {
                throw new FormatException($"Invalid time format: {request.Horario}");
            }
            return new Missa{
                DiaSemana = request.DiaSemana,
                Horario = horario            
            };
        }
    }

}