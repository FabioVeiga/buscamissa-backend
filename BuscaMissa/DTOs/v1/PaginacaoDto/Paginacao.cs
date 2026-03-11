using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuscaMissa.DTOs.PaginacaoDto
{
    public class Paginacao<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public bool HasPrevieusPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
        public int NextPage => HasNextPage == true ? PageIndex + 1 : PageIndex;
        public int PrevieusPage => HasPrevieusPage == true ? PageIndex - 1 : PageIndex;
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
        public IList<T> Items { get; set; }

        public Paginacao(int pageIndex, int pageSize, int totalItems, List<T> items)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalItems = totalItems;
            Items = items;
        }
    }
}