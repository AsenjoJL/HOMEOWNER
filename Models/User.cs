using System.ComponentModel.DataAnnotations;

namespace HOMEOWNER.Models
{
    public class User
    {
        public int UserID { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty; // ✅ Default value set

        [Required, EmailAddress]
        public required string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty; // ✅ Default value set

        [Phone]
        public string? Phone { get; set; } // ✅ Nullable to prevent errors

        [Required]
        public string PropertyAddress { get; set; } = string.Empty; // ✅ Default value set


        [Required]
        public string BlockLotNumber { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User"; // ✅ Default role is User

        public DateTime CreatedAt { get; set; } = DateTime.Now; // ✅ Default timestamp
    }
}
