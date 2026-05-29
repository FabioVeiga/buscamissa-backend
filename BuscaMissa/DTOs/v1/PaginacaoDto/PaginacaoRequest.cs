using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.PaginacaoDto
{
    public class PaginacaoRequest
    {
        [Required(ErrorMessage = "{0} é obrigatório!")]
        public int PageIndex { get; set; } = 1;

        [Required(ErrorMessage = "{0} é obrigatório!")]
        public int PageSize { get; set; } = 10;
    }
}