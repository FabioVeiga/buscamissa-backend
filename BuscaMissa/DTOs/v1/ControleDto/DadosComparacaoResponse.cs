using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.ControleDto
{
    // Um lado do comparativo antes/depois (dados atuais da igreja OU dados propostos
    // vindos de IgrejaTemporaria/MissaTemporaria — para criação, o "atual" fica nulo).
    public class DadosComparacaoResponse
    {
        public string? Nome { get; set; }
        public string? Paroco { get; set; }
        public string? ImagemUrl { get; set; }
        public EnderecoIgrejaResponse? Endereco { get; set; }
        public IList<MissaComparacaoItem> Missas { get; set; } = [];
    }

    public class MissaComparacaoItem
    {
        // Numérico (mesmo valor de DiaDaSemanaEnum) — o frontend já converte para label.
        public DiaDaSemanaEnum DiaSemana { get; set; }
        public string Horario { get; set; } = default!;
        public string? Observacao { get; set; }
    }
}
