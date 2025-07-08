using HOMEOWNER.Data;
using HOMEOWNER.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace HOMEOWNER.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var homeowner = _context.Homeowners.FirstOrDefault(h => h.Email == model.Email);
            if (homeowner == null)
            {
                ViewBag.ErrorMessage = "Email not found.";
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                ViewBag.ErrorMessage = "Password cannot be empty.";
                return View(model);
            }

            // Generate salt
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);

            // Hash the new password
            var hashedPasswordBytes = KeyDerivation.Pbkdf2(
                password: model.NewPassword,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32);

            var hashedPassword = Convert.ToBase64String(hashedPasswordBytes);

            // Save new password
            homeowner.PasswordHash = $"{salt}:{hashedPassword}";
            _context.SaveChanges();

            ViewBag.SuccessMessage = "Password reset successful! You can now log in.";
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if the email matches an admin
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == model.Email);
            if (admin != null && VerifyPassword(model.Password, admin.PasswordHash))
            {
                var claims = new List<Claim>
        {
            new(ClaimTypes.Name, admin.FullName ?? "Unknown Admin"),
            new(ClaimTypes.Email, admin.Email ?? "unknown@domain.com"),
            new(ClaimTypes.Role, "Admin")
        };

                await SignInUser(claims);
                return RedirectToAction("Dashboard", "Admin");
            }

            // Check if the email matches a homeowner
            var homeowner = await _context.Homeowners.FirstOrDefaultAsync(h => h.Email == model.Email);
            if (homeowner != null && VerifyPassword(model.Password, homeowner.PasswordHash))
            {
                var claims = new List<Claim>
        {
            new(ClaimTypes.Name, homeowner.FullName ?? "Unknown Homeowner"),
            new(ClaimTypes.Email, homeowner.Email ?? "unknown@domain.com"),
            new(ClaimTypes.Role, "Homeowner")
        };

                await SignInUser(claims);

                // Store HomeownerID in session
                HttpContext.Session.SetInt32("HomeownerID", homeowner.HomeownerID);
                return RedirectToAction("Dashboard", "Homeowner");
            }

            // Check if the email matches a staff member
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Email == model.Email);
            if (staff != null && VerifyPassword(model.Password, staff.PasswordHash))
            {
                var claims = new List<Claim>
        {
            new(ClaimTypes.Name, staff.FullName ?? "Unknown Staff"),
            new(ClaimTypes.Email, staff.Email ?? "unknown@domain.com"),
            new(ClaimTypes.Role, "Staff"),
            new("Position", staff.Position ?? "Unknown") // Set "Position" instead of "Role"
        };

                await SignInUser(claims);

                // Store staff info in session
                HttpContext.Session.SetString("StaffRole", staff.Position ?? "Unknown");
                HttpContext.Session.SetInt32("StaffID", staff.StaffID);
                HttpContext.Session.SetString("StaffName", staff.FullName ?? "Unknown Staff");

                return RedirectToAction("Dashboard", "Staff");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // ✅ Helper method to sign in user
        private async Task SignInUser(List<Claim> claims)
        {
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                          new ClaimsPrincipal(claimsIdentity),
                                          authProperties);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // ✅ Clear session on logout
            return RedirectToAction("Index", "Home");
        }

        private static bool VerifyPassword(string? enteredPassword, string? storedHash)
        {
            if (string.IsNullOrWhiteSpace(enteredPassword) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            string[] parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            byte[] salt, storedHashBytes;

            try
            {
                salt = Convert.FromBase64String(parts[0]);
                storedHashBytes = Convert.FromBase64String(parts[1]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] enteredHashBytes = KeyDerivation.Pbkdf2(
                password: enteredPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32
            );

            return enteredHashBytes.SequenceEqual(storedHashBytes);
        }
    }
}
