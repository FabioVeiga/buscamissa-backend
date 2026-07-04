using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.UsuarioDto
{
    public class UsuarioFiltroRequest
    {
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public bool? Bloqueado { get; set; }
        public PerfilEnum? Perfil { get; set; }
        public DateTime? CriacaoInicio { get; set; }
        public DateTime? CriacaoFim { get; set; }
        public PaginacaoRequest Paginacao { get; set; } = default!;
    }
}