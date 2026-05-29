using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuscaMissa.Models;

[Table("ComentariosIgreja")]
public class ComentarioIgreja
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public int IgrejaId { get; set; }
    public Igreja Igreja { get; set; } = null!;

    [MaxLength(80)]
    public string Nome { get; set; }

    [Required]
    [MaxLength(500)]
    public string Comentario { get; set; }

    [MaxLength(200)]
    public string HashFingerprint { get; set; }

    [MaxLength(45)]
    public string EnderecoIp { get; set; }

    public bool Aprovado { get; set; }

    [MaxLength(200)]
    public string MotivoBloqueio { get; set; }

    public DateTime DataCriacao { get; set; }
}