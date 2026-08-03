using System;
using System.Linq;
using System.Threading.Tasks;
using AssetManagementDemo.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementDemo.Web.Helpers
{
    public static class QueryableExtensions
    {
        private static readonly int[] AllowedPageSizes = new[] { 10, 20, 50, 100 };

        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? sortBy = null,
            bool sortDescending = false)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            if (!AllowedPageSizes.Contains(pageSize))
            {
                pageSize = 10;
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>(items, totalItems, pageNumber, pageSize, searchTerm, sortBy, sortDescending);
        }
    }
}
