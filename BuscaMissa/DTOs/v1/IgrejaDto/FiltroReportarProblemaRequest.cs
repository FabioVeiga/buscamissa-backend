using BuscaMissa.DTOs.PaginacaoDto;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class FiltroReportarProblemaRequest
    {
        // Sem informar, lista só os pendentes (AcaoRealizada vazio) — mesmo padrão
        // já usado no filtro de Igrejas (FiltroIgrejaAdminRequest.ReportarProblema).
        public bool? Resolvido { get; set; }
        public PaginacaoRequest Paginacao { get; set; } = new();
    }
}
