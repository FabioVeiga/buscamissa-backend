using BuscaMissa.DTOs.MissaDto;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class AtualizacaoIgrejaResponse
    {
        public int Id { get; set; }        
        public string? Paroco { get; set; }
        public string? ImagemUrl { get; set; }
        public ICollection<MissaResponse> MissasTemporaria { get; set; } = [];
    }
}