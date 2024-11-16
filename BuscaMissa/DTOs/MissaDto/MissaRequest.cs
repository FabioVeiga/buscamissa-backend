using System.ComponentModel.DataAnnotations;
using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.MissaDto
{
    public class MissaRequest : IValidatableObject
    {
        public int? Id { get; set; }
        [Required]
        public DiaDaSemanaEnum DiaDaSemana { get; set; }
        [Required]
        public string Horario { get; set; } = default!;
        internal TimeSpan HorarioMissa { get; set; } = default!;
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
                    HorarioMissa = horario;
            }
            
            if(!Enum.IsDefined(typeof(DiaDaSemanaEnum), DiaDaSemana))
            {
                results.Add(new ValidationResult("Dia da semana invalido.", [nameof(DiaDaSemana)]));
            }
            return results;
        }
    }
}