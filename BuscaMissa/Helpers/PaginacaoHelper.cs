using BuscaMissa.DTOs.PaginacaoDto;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Helpers
{
    public static class PaginacaoHelper
    {
        public static async Task<Paginacao<T>> PaginacaoAsync<T>(this IQueryable<T> queryable, int paginaIndex, int pageSize)
        {
            var totalCount = await queryable.CountAsync();
            var items = await queryable.Skip((paginaIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new Paginacao<T>(paginaIndex, pageSize, totalCount, items);
        }

        public static Paginacao<T> Paginacao<T>(this ICollection<T> list, int pageIndex, int pageSize)
        {
            var totalCount = list.Count;
            var items = list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new Paginacao<T>(pageIndex, pageSize, totalCount, items);
        }
    }
}