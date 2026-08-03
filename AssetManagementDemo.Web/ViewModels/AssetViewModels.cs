using System;
using System.ComponentModel.DataAnnotations;

namespace AssetManagementDemo.Web.ViewModels
{
    public class AssetCreateViewModel
    {
        [Required(ErrorMessage = "Asset Code is required")]
        [StringLength(20)]
        [Display(Name = "Asset Code")]
        public string AssetCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Asset Name is required")]
        [StringLength(100)]
        [Display(Name = "Asset Name")]
        public string AssetName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(50)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        [StringLength(100)]
        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Purchase Date")]
        public DateTime? PurchaseDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Warranty Expiry")]
        public DateTime? WarrantyExpiry { get; set; }

        [Range(0, 100000000, ErrorMessage = "Price must be positive")]
        [Display(Name = "Purchase Price ($)")]
        public decimal? PurchasePrice { get; set; }

        [Required]
        public string Status { get; set; } = "Available"; // Available, Assigned, Under Repair, Retired
    }

    public class AssetEditViewModel : AssetCreateViewModel
    {
        public int AssetId { get; set; }
    }
}
