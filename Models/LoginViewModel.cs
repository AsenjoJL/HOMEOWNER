using System.ComponentModel.DataAnnotations;

namespace HOMEOWNER.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }  // Use Email instead of Username

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; } = string.Empty;  // Allow null values
        public string? UserType { get; set; }
    }
}
