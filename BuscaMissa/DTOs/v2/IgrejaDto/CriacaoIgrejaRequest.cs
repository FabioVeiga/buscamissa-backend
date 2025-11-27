using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.v2.IgrejaDto;

public class CriacaoIgrejaRequest : BuscaMissa.DTOs.IgrejaDto.CriacaoIgrejaRequest
{
    [Required] 
    public string NomeUnico { get; set; } = null!;
}