using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.MissaDto;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class CriacaoIgrejaRequest
    {
        [Required]
        public string Nome { get; set; } = default!;
        public string? Paroco { get; set; }
        public string? Imagem { get; set; }
        public ICollection<MissaRequest> Missas { get; set; } = [];
        public EnderecoIgrejaRequest Endereco { get; set; } = default!;
        public IgrejaContatoRequest? Contato { get; set; }
    }
}