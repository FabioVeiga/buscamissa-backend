using BuscaMissa.Models;

namespace BuscaMissa.DTOs.UsuarioDto
{
    public class UsuarioResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public string Email { get; set; } = default!;
        public AcessToken? AcessToken { get; set; }

        public static explicit operator UsuarioResponse(Usuario usuario)
        {
            return new UsuarioResponse{
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email
            };
        }
    }
}