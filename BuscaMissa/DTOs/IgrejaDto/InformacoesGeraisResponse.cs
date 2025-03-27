
namespace BuscaMissa.DTOs.IgrejaDto
{
    public class InformacoesGeraisResponse
    {
        public int QuantidadesIgrejas { get; set; } = 0;
        public int QuantidadeMissas { get; set; } = 0;
        public int QuantidadeIgrejaDenunciaNaoAtendida { get; set; } = 0;
        public int QuantidadeSolicitacoesNaoAtendida { get; set; } = 0;
        public int QuantidadeDeUsuarios { get; set; } = 0;
    }
}