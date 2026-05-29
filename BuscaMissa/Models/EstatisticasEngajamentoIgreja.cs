using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuscaMissa.Models;

[Table("EstatisticasEngajamentoIgreja")]
public class EstatisticasEngajamentoIgreja
{
    [Key]
    [ForeignKey("Igreja")]
    public int IgrejaId { get; set; }
    public Igreja Igreja { get; set; } = null!;

    public int TotalCurtidas { get; set; }

    public int TotalAvaliacoes { get; set; }

    public double MediaAvaliacoes { get; set; }

    public int TotalComentarios { get; set; }

    public int TotalVisualizacoes { get; set; }

    public DateTime UltimaAtualizacao { get; set; }
}