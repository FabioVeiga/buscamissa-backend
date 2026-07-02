using BuscaMissa.Models;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class ReportarProblemaAdminResponse
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = default!;
        public string Nome { get; set; } = default!;
        public string Email { get; set; } = default!;

          public static explicit operator ReportarProblemaAdminResponse(IgrejaReportarProblema problemaReportado)
          {
            return new ReportarProblemaAdminResponse()
            {
                Descricao = problemaReportado.Descricao,
                Id = problemaReportado.Id,
                Nome = problemaReportado.Nome,
                Email = problemaReportado.Email
            };
          }
    }
}