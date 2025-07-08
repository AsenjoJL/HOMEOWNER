using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HOMEOWNER.Models
{
    public class Staff
    {
        [Key] // Ensure this is the primary key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
        public int StaffID { get; set; }


        [Required]
        public string? FullName { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? PhoneNumber { get; set; }
        public string? Position { get; set; }  // maintenance, security
        [Required]

        public string? PasswordHash { get; set; }
    }
}