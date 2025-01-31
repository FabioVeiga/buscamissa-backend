using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class FiltroIgrejaRequest : IValidatableObject
    {
        [Required]
        public string Uf { get; set; } = default!;
        public string? Localidade { get; set; }
        public string? Bairro { get; set; }
        public string? Nome { get; set; }
        public bool Ativo { get; set; } = true;
        public DiaDaSemanaEnum? DiaDaSemana { get; set; }
        public string? Horario { get; set; }
        internal TimeSpan? HorarioMissa { get; set; }
        public PaginacaoRequest Paginacao { get; set; } = default!;

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
            
            if(DiaDaSemana is not null && !Enum.IsDefined(typeof(DiaDaSemanaEnum), DiaDaSemana))
            {
                results.Add(new ValidationResult("Dia da semana invalido.", [nameof(DiaDaSemana)]));
            }
            return results;
        }
    }
}