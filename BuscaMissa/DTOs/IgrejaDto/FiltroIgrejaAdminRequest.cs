using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.Enums;
using BuscaMissa.Filters;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class FiltroIgrejaAdminRequest : IValidatableObject
    {
        public string? Uf { get; set; } = default!;
        public string? Localidade { get; set; }
        public string? Bairro { get; set; }
        [NoProfanity]
        public string? Nome { get; set; }
        public bool Ativo { get; set; } = true;
        public DiaDaSemanaEnum? DiaDaSemana { get; set; }
        public string? Horario { get; set; }
        internal TimeSpan? HorarioMissa { get; set; }
        public PaginacaoRequest Paginacao { get; set; } = default!;
        public bool Solicitacao { get; set; } = false;
        public bool Denuncia { get; set; } = false;

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