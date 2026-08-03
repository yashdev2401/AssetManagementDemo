using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagementDemo.Web.Models
{
    [Table("Assets")]
    public class Asset
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AssetId { get; set; }

        [Required]
        [StringLength(20)]
        public string AssetCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string AssetName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(50)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        [StringLength(100)]
        public string? SerialNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PurchaseDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? WarrantyExpiry { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? PurchasePrice { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Available"; // e.g. Available, Assigned, Under Repair, Retired

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        // Navigation Properties
        public virtual ICollection<AssetAssignment> AssetAssignments { get; set; } = new List<AssetAssignment>();
    }
}
