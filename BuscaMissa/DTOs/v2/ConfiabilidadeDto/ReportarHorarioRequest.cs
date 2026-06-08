using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.v2.ConfiabilidadeDto;

public class ReportarHorarioRequest
{
    [Required]
    [MaxLength(200)]
    public string Fingerprint { get; set; } = null!;

    /// <summary>Flags combinados dos motivos selecionados</summary>
    [Range(1, 15)]
    public int Motivos { get; set; }

    [MaxLength(1000)]
    public string? Descricao { get; set; }

    [MaxLength(500)]
    public string? FonteInformacao { get; set; }
}
