using System.ComponentModel.DataAnnotations;
using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.v1.EmailEventoIgrejaDto;

public class RegistrarContatoRequest
{
    [Required] public int IgrejaId { get; set; }

    [Required] public TipoEmailEventoIgrejaEnum Tipo { get; set; }

    [Required] public CanalContatoEnum Canal { get; set; }

    [Required] [MaxLength(300)] public string DestinoContato { get; set; } = string.Empty;

    [Required] public DateTime DataEnvio { get; set; }

    [MaxLength(500)] public string? Observacao { get; set; }
}
