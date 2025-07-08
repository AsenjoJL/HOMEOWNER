using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HOMEOWNER.Models
{
    public class HomeownerProfileImage
    {
        [Key]
        public int ImageID { get; set; }

        [Required]
        public int HomeownerID { get; set; }

        [Required]
        [StringLength(500)]
        public string? ImagePath { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // ✅ Add fields to track change limit
        public int ChangeCount { get; set; } = 0;
        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow.Date;

        [ForeignKey("HomeownerID")]
        public virtual Homeowner? Homeowner { get; set; }
    }
}
