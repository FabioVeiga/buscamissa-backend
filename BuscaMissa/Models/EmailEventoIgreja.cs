using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuscaMissa.Enums;

namespace BuscaMissa.Models;

[Table("EmailEventosIgreja")]
public class EmailEventoIgreja
{
    [Key] public int Id { get; set; }

    [Required] public int IgrejaId { get; set; }

    public Igreja Igreja { get; set; } = null!;

    [Required] public TipoEmailEventoIgrejaEnum Tipo { get; set; }

    [Required] [MaxLength(200)] public string Assunto { get; set; } = null!;

    [Required] [MaxLength(255)] public string EmailDestino { get; set; } = null!;

    [MaxLength(120)] public string? NomeDestino { get; set; }

    [Required] public string Html { get; set; } = null!;

    public bool Ativo { get; set; } = true;

    public bool Enviado { get; set; }

    public DateTime? DataEnvio { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DataAlteracao { get; set; }

    [MaxLength(500)] public string? Observacao { get; set; }

    public CanalContatoEnum Canal { get; set; } = CanalContatoEnum.Email;

    [MaxLength(300)] public string? DestinoContato { get; set; }
}