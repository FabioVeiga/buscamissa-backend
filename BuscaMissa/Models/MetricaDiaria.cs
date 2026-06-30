using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuscaMissa.Enums;

namespace BuscaMissa.Models;

[Table("MetricasDiarias")]
public class MetricaDiaria
{
    [Key]
    public int Id { get; set; }

    [Required]
    public TipoEntidadeMetricaEnum TipoEntidade { get; set; }

    [Required]
    public int EntidadeId { get; set; }

    [Required]
    public TipoMetricaEnum TipoMetrica { get; set; }

    [Required]
    public DateOnly Data { get; set; }

    [Required]
    public int Quantidade { get; set; } = 0;

    [Required]
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}
