using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.MissaDto;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class AtualizacaoIgrejaResponse
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Paroco { get; set; }
        public string? ImagemUrl { get; set; }
        public ICollection<MissaResponse> MissasTemporaria { get; set; } = [];
        public EnderecoIgrejaResponse Endereco { get; set; } = default!;
    }
}