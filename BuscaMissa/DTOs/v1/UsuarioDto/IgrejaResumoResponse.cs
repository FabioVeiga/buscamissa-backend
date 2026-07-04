namespace BuscaMissa.DTOs.UsuarioDto
{
    public class IgrejaResumoResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public string? Uf { get; set; }
        public string? Localidade { get; set; }
    }
}
