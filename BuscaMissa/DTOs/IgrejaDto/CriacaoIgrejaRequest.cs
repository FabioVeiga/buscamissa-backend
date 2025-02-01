using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.MissaDto;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class CriacaoIgrejaRequest
    {
        [Required]
        public string Nome { get; set; } = default!;
        public string? Paroco { get; set; }
        public string? Imagem { get; set; }
        public ICollection<MissaRequest> Missas { get; set; } = [];
        public EnderecoIgrejaRequest Endereco { get; set; } = default!;
        public CriacaoIgrejaContatoRequest? Contato { get; set; }
    }

    public class CriacaoIgrejaContatoRequest : IValidatableObject
    {
        public string? EmailContato { get; set; }
        public string? DDD { get; set; }
        public string? Telefone { get; set; }
        public string? DDDWhatsApp { get; set; }
        public string? TelefoneWhatsApp { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (DDDWhatsApp is not null && TelefoneWhatsApp is not null)
            {
                var normalizar = Helpers.TelefoneHelper.NormalizarTelefone($"{DDDWhatsApp}{TelefoneWhatsApp}");
                var isValido = Helpers.TelefoneHelper.ValidarCelular(normalizar);
                if(!isValido)
                    results.Add(new ValidationResult("Não são válidos.", [nameof(DDDWhatsApp),nameof(TelefoneWhatsApp)]));
            }
            return results;
        }
    }
}