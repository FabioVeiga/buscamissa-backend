using System.ComponentModel.DataAnnotations;
using BuscaMissa.Enums;
using BuscaMissa.Filters;

namespace BuscaMissa.DTOs.MissaDto
{
    public class MissaRequest : IValidatableObject
    {
        public int? Id { get; set; }
        [Required]
        public DiaDaSemanaEnum DiaSemana { get; set; }
        [Required]
        public string Horario { get; set; } = default!;
        internal TimeSpan HorarioMissa { get; set; } = default!;
        [NoProfanity]
        public string? Observacao { get; set; }
        internal int IgrejaId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if(!string.IsNullOrEmpty(Horario))
            {
                if (!TimeSpan.TryParse(Horario, out var horario))
                    results.Add(new ValidationResult("Formato do horario invalido.", [nameof(Horario)]));
                else
                    HorarioMissa = new TimeSpan(horario.Hours, horario.Minutes, 0);
            }
            
            if(!Enum.IsDefined(typeof(DiaDaSemanaEnum), DiaSemana))
            {
                results.Add(new ValidationResult("Dia da semana invalido.", [nameof(DiaSemana)]));
            }
            return results;
        }
    }
}