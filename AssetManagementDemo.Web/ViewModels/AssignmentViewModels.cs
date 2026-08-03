using System;
using System.ComponentModel.DataAnnotations;

namespace AssetManagementDemo.Web.ViewModels
{
    public class AssignAssetViewModel
    {
        [Required(ErrorMessage = "Employee selection is required")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Asset selection is required")]
        [Display(Name = "Asset")]
        public int AssetId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Assigned Date")]
        public DateTime AssignedDate { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class ReturnAssetViewModel
    {
        public int AssignmentId { get; set; }
        public string? EmployeeName { get; set; }
        public string? AssetName { get; set; }
        public string? AssetCode { get; set; }
        public DateTime AssignedDate { get; set; }

        [Required(ErrorMessage = "Return Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Return Date")]
        public DateTime ReturnDate { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
