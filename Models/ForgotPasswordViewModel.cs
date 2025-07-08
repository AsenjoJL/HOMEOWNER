using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace HOMEOWNER.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [PasswordStrength(ErrorMessage = "Password is too weak. It must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }
    }

    public class PasswordStrengthAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationResult("Password is required.");

            string password = value.ToString()!;

            // Check password strength
            bool hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
            bool hasLowerCase = Regex.IsMatch(password, @"[a-z]");
            bool hasDigit = Regex.IsMatch(password, @"\d");
            bool hasSpecialChar = Regex.IsMatch(password, @"[@$!%*?&]");
            bool isLongEnough = password.Length >= 8;

            if (!isLongEnough)
                return new ValidationResult("Password must be at least 8 characters long.");

            if (hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar)
                return ValidationResult.Success; // Strong password

            if ((hasUpperCase && hasLowerCase && hasDigit) || (hasUpperCase && hasLowerCase && hasSpecialChar))
                return new ValidationResult("Password strength: Moderate. Consider adding a special character or digit for a stronger password.");

            return new ValidationResult("Password is weak. It must contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
        }
    }
}