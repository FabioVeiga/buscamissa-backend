using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.v2.ConfiabilidadeDto;

public class ConfirmarHorarioRequest
{
    [Required]
    [MaxLength(200)]
    public string Fingerprint { get; set; } = null!;
}
