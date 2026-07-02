namespace BuscaMissa.DTOs.v1.DivulgacaoDto;

public class IgrejaDivulgacaoResponse
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string? Cidade { get; set; }

    public string? CidadeSlug { get; set; }

    public string? Uf { get; set; }

    public string? Slug { get; set; }

    public string? Email { get; set; }

    public string? Instagram { get; set; }

    public string? Facebook { get; set; }

    public DateTime Criacao { get; set; }

    public DateTime Alteracao { get; set; }

    public DateTime? UltimoContatoEmail { get; set; }

    public DateTime? UltimoContatoFacebook { get; set; }

    public DateTime? UltimoContatoInstagram { get; set; }
}
