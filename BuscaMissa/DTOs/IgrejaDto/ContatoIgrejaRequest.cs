using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class IgrejaContatoRequest : IValidatableObject
    {
        public string? EmailContato { get; set; }
        public string? DDD { get; set; }
        public string? Telefone { get; set; }
        public string? DDDWhatsApp { get; set; }
        public string? TelefoneWhatsApp { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (!string.IsNullOrEmpty(EmailContato))
            {
                if(!Helpers.EmailHelper.ValidarEmail(EmailContato))
                    results.Add(new ValidationResult("Email inválido", [nameof(EmailContato)]));
            }
            if (!string.IsNullOrEmpty(DDD) && !string.IsNullOrEmpty(Telefone))
            {
                var normalizar = Helpers.TelefoneHelper.NormalizarTelefone($"{DDD}{Telefone}");
                var isValido = Helpers.TelefoneHelper.ValidarTelefone(normalizar);
                if(!isValido)
                    results.Add(new ValidationResult("Não são válidos.", [nameof(DDD),nameof(Telefone)]));
            }
            if (!string.IsNullOrEmpty(DDDWhatsApp) && !string.IsNullOrEmpty(TelefoneWhatsApp))
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