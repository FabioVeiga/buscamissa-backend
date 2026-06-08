using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuscaMissa.Enums;

namespace BuscaMissa.Models;

[Table("ReportesHorario")]
public class ReporteHorario
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public int IgrejaId { get; set; }
    public Igreja Igreja { get; set; } = null!;

    /// <summary>Flags: HorarioIncorreto=1, MissaNaoOcorreMais=2, InformacaoDesatualizada=4, OutroMotivo=8</summary>
    public int Motivos { get; set; }

    [MaxLength(1000)]
    public string? Descricao { get; set; }

    [MaxLength(500)]
    public string? FonteInformacao { get; set; }

    [Required]
    [MaxLength(200)]
    public string HashFingerprint { get; set; } = null!;

    [MaxLength(45)]
    public string? EnderecoIp { get; set; }

    public StatusReporteEnum Status { get; set; } = StatusReporteEnum.Pendente;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
