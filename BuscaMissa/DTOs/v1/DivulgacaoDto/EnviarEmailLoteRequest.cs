using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.v1.DivulgacaoDto;

public class EnviarEmailLoteRequest
{
    [Required(ErrorMessage = "{0} é obrigatório!")]
    public List<int> IgrejaIds { get; set; } = [];

    [Required(ErrorMessage = "{0} é obrigatório!")]
    public string Tipo { get; set; } = null!;
}
