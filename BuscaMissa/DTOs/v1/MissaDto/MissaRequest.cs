using System.ComponentModel.DataAnnotations;
using BuscaMissa.Enums;
using BuscaMissa.Filters;
using BuscaMissa.Helpers;

namespace BuscaMissa.DTOs.MissaDto
{
    public class MissaRequest : IValidatableObject
    {
        public int? Id { get; set; }
        [Required] public DiaDaSemanaEnum DiaSemana { get; set; }
        [Required] public string Horario { get; set; } = default!;
        internal TimeSpan HorarioMissa { get; set; } = default!;
        [NoProfanity] public string? Observacao { get; set; }
        internal int IgrejaId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (!string.IsNullOrEmpty(Horario))
            {
                if (!DataHoraHelper.TryParseHorarioMissa(Horario, out var horario))
                {
                    results.Add(new ValidationResult("Formato do horário inválido.", [nameof(Horario)]));
                }
                else
                {
                    HorarioMissa = horario;
                    Horario = horario.ToString(@"hh\:mm");
                }
            }

            if (!Enum.IsDefined(typeof(DiaDaSemanaEnum), DiaSemana))
            {
                results.Add(new ValidationResult("Dia da semana invalido.", [nameof(DiaSemana)]));
            }

            return results;
        }
    }
}