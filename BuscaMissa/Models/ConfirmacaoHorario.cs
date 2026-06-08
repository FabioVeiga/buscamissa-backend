using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuscaMissa.Models;

[Table("ConfirmacoesHorario")]
public class ConfirmacaoHorario
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public int IgrejaId { get; set; }
    public Igreja Igreja { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string HashFingerprint { get; set; } = null!;

    [MaxLength(45)]
    public string? EnderecoIp { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
