using System.ComponentModel.DataAnnotations;
using BuscaMissa.Filters;

namespace BuscaMissa.DTOs.ControleDto
{
    public class CodigoValidadorRequest
    {
        [Required]
        public int ControleId { get; set; }
        [Required]
        public int CodigoValidador { get; set; }
        [Required(ErrorMessage =  "O campo {0} é obrigatório!")]
        [EmailAddress(ErrorMessage = "Formado invalido!")]
        [NoProfanity]
        public string Email { get; set; } = null!;
    }
}