using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.Filters;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class CriacaoIgrejaRequest
    {
        [Required]
        [NoProfanity]
        public string Nome { get; set; } = default!;
        [NoProfanity]
        public string? Paroco { get; set; }
        public string? Imagem { get; set; }
        public ICollection<MissaRequest> Missas { get; set; } = [];
        [Required]
        public EnderecoIgrejaRequest Endereco { get; set; } = default!;
        public IgrejaContatoRequest? Contato { get; set; }
        public IList<RedeSolcialIgrejaRequest>? RedeSociais { get; set; }
    }
}