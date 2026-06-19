using BuscaMissa.DTOs.MissaDto;

namespace BuscaMissa.DTOs.v2.IgrejaDto;

public class ProximaMissaDto
{
    public int IgrejaId { get; set; }
    public string Nome { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Uf { get; set; } = default!;
    public string CidadeSlug { get; set; } = default!;
    public string Bairro { get; set; } = default!;
    public string? ImagemUrl { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    /// <summary>A missa com menor minutosParaInicio dentro da janela temporal. Confiança já preenchida.</summary>
    public MissaResponse Missa { get; set; } = default!;

    public int MinutosParaInicio { get; set; }
    public double DistanciaKm { get; set; }
}
