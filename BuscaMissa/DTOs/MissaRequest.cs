using System.ComponentModel.DataAnnotations;
using BuscaMissa.Enums;

namespace BuscaMissa.DTOs
{
    public class MissaRequest
    {
        [Required]
        public DiaDaSemanaEnum DiaSemana { get; set; }
        [Required]
        public string Horario { get; set; } = default!;
    }
}