using System.ComponentModel.DataAnnotations;

namespace HOMEOWNER.Models
{
    public class Admin
    {
        [Key]
        public int AdminID { get; set; } // 🔹 Ensure consistency in naming

        [Required]
        public string OfficeLocation { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Active";

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Admin";
    }
}
