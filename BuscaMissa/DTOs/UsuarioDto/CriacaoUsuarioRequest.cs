using System.ComponentModel.DataAnnotations;
using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.UsuarioDto
{
    public class CriacaoUsuarioRequest : IValidatableObject
    {
        [Required(ErrorMessage =  "O campo {0} é obrigatório!")]
        [EmailAddress(ErrorMessage = "Formado invalido!")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage =  "O campo {0} é obrigatório!")]
        public string Nome { get; set; } = null!;
        [Required(ErrorMessage =  "O campo {0} é obrigatório!")]
        [MinLength(3, ErrorMessage = "{0} deve ter no mínimo {1}")]
        [MaxLength(6, ErrorMessage = "{0} deve ter no mínimo {1}")]
        public string Senha { get; set; } = null!;
        [Required(ErrorMessage =  "O campo {0} é obrigatório!")]
        public PerfilEnum Perfil { get; set; }
        [Required(ErrorMessage =  "O campo {0} é obrigatório!")]
        public bool AceitarTermo { get; set; }
        public bool? AceitarPromocao { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (!AceitarTermo)
            {
                if(Perfil == PerfilEnum.Regular || Perfil == PerfilEnum.Dono)
                    results.Add(new ValidationResult("É necessário aceitar os termos para continuar.", [nameof(AceitarTermo)]));
            }
            return results;
        }

    }
}