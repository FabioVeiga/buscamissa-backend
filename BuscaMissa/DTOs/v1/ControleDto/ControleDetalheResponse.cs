using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.ControleDto
{
    public class ControleDetalheResponse
    {
        public int ControleId { get; set; }
        public StatusEnum Status { get; set; }
        public DateTime DataCriacao { get; set; }
        public string Tipo { get; set; } = default!;
        public int? IgrejaId { get; set; }

        // Nulo para criação (não há "antes" — a igreja ainda não existe publicamente).
        public DadosComparacaoResponse? DadosAtuais { get; set; }
        public DadosComparacaoResponse DadosPropostos { get; set; } = default!;
    }
}
