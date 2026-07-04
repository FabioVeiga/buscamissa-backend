using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.UsuarioDto
{
    public class TrocarSenhaRequest
    {
        [Required(ErrorMessage = "{0} é obrigatório!")]
        public string SenhaAtual { get; set; } = default!;

        [Required(ErrorMessage = "{0} é obrigatório!")]
        [MinLength(6, ErrorMessage = "{0} deve ter no mínimo {1} caracteres.")]
        public string NovaSenha { get; set; } = default!;
    }
}
