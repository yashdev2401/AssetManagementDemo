using System;
using System.Collections.Generic;

namespace AssetManagementDemo.Web.ViewModels
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }

        public PagedResult()
        {
        }

        public PagedResult(List<T> items, int totalItems, int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, bool sortDescending = false)
        {
            Items = items;
            TotalItems = totalItems;
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize < 1 ? 10 : pageSize;
            SearchTerm = searchTerm;
            SortBy = sortBy;
            SortDescending = sortDescending;
        }
    }
}
