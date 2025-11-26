using System.ComponentModel.DataAnnotations;
using BuscaMissa.Filters;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class IgrejaContatoRequest : IValidatableObject
    {
        [NoProfanity]
        public string? EmailContato { get; set; }
        [RegularExpression(@"^\d{2}$", ErrorMessage = "DDD deve conter apenas números e ter 2 dígitos.")]
        public string? DDD { get; set; }
        [RegularExpression(@"^\d{8}$", ErrorMessage = "O Telefone deve conter apenas números e ter 9 dígitos.")]
        public string? Telefone { get; set; }
        [RegularExpression(@"^\d{2}$", ErrorMessage = "DDDWhatsApp deve conter apenas números e ter 2 dígitos.")]
        public string? DDDWhatsApp { get; set; }
        [RegularExpression(@"^\d{9}$", ErrorMessage = "O TelefoneWhatsApp deve conter apenas números e ter 9 dígitos.")]
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