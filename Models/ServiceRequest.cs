using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HOMEOWNER.Models
{
    public class ServiceRequest
    {
        [Key]
        public int RequestID { get; set; }

        [Required]
        [ForeignKey("Homeowner")]
        public int HomeownerID { get; set; }
        public virtual Homeowner? Homeowner { get; set; }

        [Required]
        [StringLength(50)]
        public string? Category { get; set; }

        [Required]
        [StringLength(20)]
        public string? Priority { get; set; }

        [Required]
        [StringLength(255)]
        public string? Description { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? RequestType { get; set; }

        // Add the AssignedStaffID property
        public int? AssignedStaffID { get; set; }
        public DateTime? DeletedAt { get; set; } // Track when it's deleted (optional)

        public DateTime? CompletedAt { get; set; }


        // Add a navigation property to Staff (optional)
        public Staff? AssignedStaff { get; set; }

    }
}
