using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.UsuarioDto
{
    public class UsuarioBloqueadoRequest
    {
        [Required]
        public bool Bloqueado { get; set; }
        public string? MotivoBloqueio { get; set; } = default!;
    }
}