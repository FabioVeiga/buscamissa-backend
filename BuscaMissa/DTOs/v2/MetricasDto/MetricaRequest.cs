using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.v2.MetricasDto;

public class MetricaRequest
{
    [Required]
    public int EntidadeId { get; set; }
}
