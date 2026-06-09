namespace BuscaMissa.DTOs.v2.IgrejaDto;

public class SitemapIgrejaDto
{
    public string NomeUnico { get; set; } = default!;
    public string? Uf { get; set; }
    public string? CidadeSlug { get; set; }
    public string? Slug { get; set; }
    public DateTime Alteracao { get; set; }
}
