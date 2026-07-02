using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.v1.DivulgacaoDto;

public class FiltroIgrejaDivulgacaoRequest : PaginacaoRequest
{
    public ModoDivulgacaoEnum Modo { get; set; }

    public string? Nome { get; set; }

    public string? Cidade { get; set; }

    public string? Uf { get; set; }
}
