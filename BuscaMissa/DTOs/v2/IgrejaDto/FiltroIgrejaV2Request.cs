using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.Enums;
using BuscaMissa.Filters;
using BuscaMissa.Helpers;

namespace BuscaMissa.DTOs.v2.IgrejaDto;

public class FiltroIgrejaV2Request : IValidatableObject
{
    [Required]
    public string Uf { get; set; } = default!;
    public string? Localidade { get; set; }
    public IList<string>? Bairro { get; set; }
    [NoProfanity]
    public string? Nome { get; set; }
    public bool Ativo { get; set; } = true;
    public DiaDaSemanaEnum? DiaDaSemana { get; set; }
    public PeriodoEnum? Periodo { get; set; }
    public IList<string> Horarios { get; set; } = [];
    internal IList<TimeSpan>? HorarioMissa { get; set; } = new List<TimeSpan>();
    internal (TimeSpan De, TimeSpan Ate)? FaixaPeriodo { get; private set; }
    public PaginacaoRequest Paginacao { get; set; } = default!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        if (Horarios.Count > 0)
        {
            foreach (var item in Horarios)
            {
                if (!TimeSpan.TryParse(item, out var horario))
                    results.Add(new ValidationResult("Formato do horario invalido.", [nameof(Horarios)]));
                else
                    HorarioMissa.Add(horario);
            }
        }

        if (DiaDaSemana is not null && !Enum.IsDefined(typeof(DiaDaSemanaEnum), DiaDaSemana))
            results.Add(new ValidationResult("Dia da semana invalido.", [nameof(DiaDaSemana)]));

        if (Periodo is not null)
        {
            if (!Enum.IsDefined(typeof(PeriodoEnum), Periodo))
                results.Add(new ValidationResult("Período inválido. Use 1=Manhã, 2=Tarde, 3=Noite.", [nameof(Periodo)]));
            else
                FaixaPeriodo = PeriodoHelper.ObterFaixa(Periodo.Value);
        }

        return results;
    }
}