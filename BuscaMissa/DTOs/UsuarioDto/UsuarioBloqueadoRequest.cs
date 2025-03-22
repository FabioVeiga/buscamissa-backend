using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.UsuarioDto
{
    public class UsuarioBloqueadoRequest
    {
        [Required]
        public bool Bloqueado { get; set; }
        [Required]
        public string MotivoBloqueio { get; set; } = default!;
    }
}