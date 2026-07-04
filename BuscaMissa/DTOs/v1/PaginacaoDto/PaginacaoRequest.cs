using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.PaginacaoDto
{
    public class PaginacaoRequest
    {
        /// <summary>Teto de itens por página imposto no servidor (nunca confiar no cliente) — 3.J.</summary>
        public const int MaxPageSize = 50;

        private int _pageSize = 10;

        [Required(ErrorMessage = "{0} é obrigatório!")]
        public int PageIndex { get; set; } = 1;

        [Required(ErrorMessage = "{0} é obrigatório!")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : (value > MaxPageSize ? MaxPageSize : value);
        }
    }
}