using System;
using System.ComponentModel.DataAnnotations;

namespace AssetManagementDemo.Web.ViewModels
{
    public class EmployeeCreateViewModel
    {
        [Required(ErrorMessage = "Employee Code is required")]
        [StringLength(20, ErrorMessage = "Code cannot exceed 20 characters")]
        [Display(Name = "Employee Code")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employee Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string EmployeeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Designation { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(150)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Joining Date")]
        public DateTime? JoiningDate { get; set; } = DateTime.Today;

        [Required]
        public string Status { get; set; } = "Active";
    }

    public class EmployeeEditViewModel : EmployeeCreateViewModel
    {
        public int EmployeeId { get; set; }
    }
}
