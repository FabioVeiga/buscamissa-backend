using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.Filters;

namespace BuscaMissa.DTOs.ControleDto
{
    // Dados ajustados pelo Admin antes de concluir uma alteração pendente
    // (a igreja em si já existe — só Paroco/Imagem/Missas fazem parte desse fluxo).
    public class AjustarAlteracaoRequest
    {
        [NoProfanity] public string? Paroco { get; set; }
        public string? Imagem { get; set; }
        public ICollection<MissaRequest> Missas { get; set; } = [];
    }
}
