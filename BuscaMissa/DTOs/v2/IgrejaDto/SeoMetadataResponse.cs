namespace BuscaMissa.DTOs.v2.IgrejaDto;

public class SeoMetadataResponse
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string CanonicalUrl { get; set; } = default!;
    public string Keywords { get; set; } = default!;
    public string? OgImage { get; set; }
}
