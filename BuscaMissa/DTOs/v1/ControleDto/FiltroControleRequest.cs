using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.ControleDto
{
    public class FiltroControleRequest
    {
        // Sem status informado, lista só os pendentes (nem Finalizado, nem Rejeitado).
        public StatusEnum? Status { get; set; }
        public PaginacaoRequest Paginacao { get; set; } = new();
    }
}
