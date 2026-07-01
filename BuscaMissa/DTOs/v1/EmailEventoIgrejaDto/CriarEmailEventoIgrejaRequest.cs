using System.ComponentModel.DataAnnotations;
using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.v1.EmailEventoIgrejaDto;

public class CriarEmailEventoIgrejaRequest
{
    [Required] public int IgrejaId { get; set; }

    [Required] public TipoEmailEventoIgrejaEnum Tipo { get; set; }

    [Required] [MaxLength(200)] public string Assunto { get; set; } = null!;

    [Required] [EmailAddress] [MaxLength(255)] public string EmailDestino { get; set; } = null!;

    [MaxLength(120)] public string? NomeDestino { get; set; }

    [Required] public string Html { get; set; } = null!;

    public bool Ativo { get; set; } = true;

    public bool Enviado { get; set; }

    public DateTime? DataEnvio { get; set; }

    [MaxLength(500)] public string? Observacao { get; set; }
}