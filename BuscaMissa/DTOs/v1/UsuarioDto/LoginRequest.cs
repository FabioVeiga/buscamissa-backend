using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.UsuarioDto
{
    public class LoginRequest
    {
        [Required(ErrorMessage =  "O campo {0} é obrigatório!")]
        [EmailAddress(ErrorMessage = "Formado invalido!")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage =  "O campo {0} é obrigatório!")]
        public string Senha { get; set; } = null!;
    }
}