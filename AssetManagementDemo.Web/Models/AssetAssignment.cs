using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagementDemo.Web.Models
{
    [Table("AssetAssignments")]
    public class AssetAssignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AssignmentId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int AssetId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public bool? IsActive { get; set; } = true;

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey(nameof(EmployeeId))]
        public virtual Employee? Employee { get; set; }

        [ForeignKey(nameof(AssetId))]
        public virtual Asset? Asset { get; set; }
    }
}
