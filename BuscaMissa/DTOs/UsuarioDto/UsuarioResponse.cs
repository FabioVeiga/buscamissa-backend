namespace BuscaMissa.DTOs.UsuarioDto
{
    public class UsuarioResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public string Email { get; set; } = default!;
        public AcessToken? AcessToken { get; set; }
    }
}