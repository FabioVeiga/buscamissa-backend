using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.Filters;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class AtualicaoIgrejaAdminRequest
    {
        [Required] public int Id { get; set; }
        [NoProfanity] public string? Paroco { get; set; }
        [NoProfanity] public string? Nome { get; set; }
        public bool? Ativo { get; set; }
        public string? Imagem { get; set; }
        [Required] public ICollection<MissaRequest> Missas { get; set; } = [];
        public EnderecoIgrejaRequest? Endereco { get; set; }
        public IgrejaContatoRequest? Contato { get; set; }
        public IList<RedeSolcialIgrejaRequest>? RedeSociais { get; set; }
    }
}