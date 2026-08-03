using System;

namespace AssetManagementDemo.McpServer.DTOs
{
    public class AssignmentDto
    {
        public int AssignmentId { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public int AssetId { get; set; }
        public string? AssetName { get; set; }
        public string? AssetCode { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? Remarks { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class AssignAssetDto
    {
        public int EmployeeId { get; set; }
        public int AssetId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.Today;
        public string? Remarks { get; set; }
    }

    public class ReturnAssetDto
    {
        public int AssignmentId { get; set; }
        public DateTime ReturnDate { get; set; } = DateTime.Today;
        public string? Remarks { get; set; }
    }

    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; }
    }

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }
}
