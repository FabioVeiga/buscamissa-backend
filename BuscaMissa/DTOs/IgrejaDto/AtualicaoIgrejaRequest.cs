using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs.MissaDto;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class AtualicaoIgrejaRequest
    {
        [Required]
        public int Id { get; set; }
        public string? Paroco { get; set; }
        public string? Imagem { get; set; }
        [Required]
        public ICollection<MissaRequest> Missas { get; set; } = [];
    }
}