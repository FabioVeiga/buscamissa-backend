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

        // Campo de ordenação: Nome, Email ou Criacao (default: Nome). Sem isso, Skip/Take
        // sem ORDER BY é não-determinístico — cada página podia repetir/pular linhas.
        public string? OrdenarPor { get; set; }
        public bool OrdemDecrescente { get; set; } = false;

        public PaginacaoRequest Paginacao { get; set; } = default!;
    }
}