namespace BuscaMissa.DTOs.IgrejaDto
{
    // Item da listagem de "Problemas Reportados" no Admin — traz o suficiente da
    // igreja pra exibir e linkar direto pra edição, sem precisar de outra chamada.
    public class ReportarProblemaListItemResponse
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = default!;
        public string? AcaoRealizada { get; set; }
        public string Nome { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime DataCriacao { get; set; }
        public int IgrejaId { get; set; }
        public string NomeIgreja { get; set; } = default!;
        public string? Cidade { get; set; }
        public string? Uf { get; set; }
    }
}
