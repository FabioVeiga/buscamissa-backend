using BuscaMissa.DTOs.PaginacaoDto;

namespace BuscaMissa.DTOs.UsuarioDto
{
    public class UsuarioFiltroRequest
    {
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public bool? Bloqueado { get; set; }
        public PaginacaoRequest Paginacao { get; set; } = default!;
    }
}