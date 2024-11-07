using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.UsuarioDto
{
    public class IgrejaCriacaoUsuarioRequest : IValidatableObject
    {
        [Required(ErrorMessage =  "O campo {0} é obrigatório!")]
        [EmailAddress(ErrorMessage = "Formado invalido!")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage =  "O campo {0} é obrigatório!")]
        public string Nome { get; set; } = null!;
        [Required(ErrorMessage =  "O campo {0} é obrigatório!")]
        public int IgrejaId { get; set; }
        [Required(ErrorMessage =  "O campo {0} é obrigatório!")]
        public bool AceitarTermo { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (!AceitarTermo)
                results.Add(new ValidationResult("É necessário aceitar os termos para continuar.", [nameof(AceitarTermo)]));
            return results;
        }

    }
}