using System;

namespace AssetManagementDemo.Web.DTOs
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
}
