using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.EnderecoDto
{
    public class EnderecoIgrejaBuscaRequest
    {
        
        public string Uf { get; set; } = default!;
        public string? Localidade { get; set; }
        public string? Bairro { get; set; }
    }
}