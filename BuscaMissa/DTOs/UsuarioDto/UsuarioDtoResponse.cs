using BuscaMissa.Models;

namespace BuscaMissa.DTOs.UsuarioDto
{
    public class UsuarioDtoResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;

        public static explicit operator UsuarioDtoResponse(Usuario usuario)
        {
            if(usuario is null) return new UsuarioDtoResponse();
            return new UsuarioDtoResponse()
            {
                Id = usuario.Id,
                Nome = usuario.Nome
            };
        }
    }
}