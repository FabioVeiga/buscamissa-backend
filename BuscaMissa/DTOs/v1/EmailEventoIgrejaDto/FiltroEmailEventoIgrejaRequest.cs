using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.v1.EmailEventoIgrejaDto;

public class FiltroEmailEventoIgrejaRequest : PaginacaoRequest
{
    public int? IgrejaId { get; set; }

    public TipoEmailEventoIgrejaEnum? Tipo { get; set; }

    public string? EmailDestino { get; set; }

    public bool? Ativo { get; set; }

    public bool? Enviado { get; set; }

    public DateTime? DataCriacaoInicio { get; set; }

    public DateTime? DataCriacaoFim { get; set; }
}