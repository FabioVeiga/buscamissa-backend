using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuscaMissa.Models;

[Table("AvaliacoesIgreja")]
public class AvaliacaoIgreja
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public int IgrejaId { get; set; }
    public Igreja Igreja { get; set; } = null!;

    [Range(1,5)]
    public int Nota { get; set; }

    [Required]
    [MaxLength(200)]
    public string HashFingerprint { get; set; }

    public DateTime DataCriacao { get; set; }
}