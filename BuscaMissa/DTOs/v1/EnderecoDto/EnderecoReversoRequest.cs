namespace BuscaMissa.DTOs.v1.EnderecoDto;

public class EnderecoReversoRequest
{
    public string Uf         { get; set; } = string.Empty;
    public string Cidade     { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string? Bairro    { get; set; }
}