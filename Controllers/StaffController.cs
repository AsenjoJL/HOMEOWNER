using HOMEOWNER.Data;
using HOMEOWNER.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HOMEOWNER.Controllers
{
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Staff/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Staff/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Email == email);

            if (staff == null || string.IsNullOrEmpty(staff.PasswordHash) || !VerifyPassword(password, staff.PasswordHash))
            {
                ViewBag.Error = "Invalid email or password!";
                return View();
            }

            // Set session values
            HttpContext.Session.SetInt32("StaffID", staff.StaffID);
            HttpContext.Session.SetString("StaffName", staff.FullName ?? string.Empty);
            HttpContext.Session.SetString("StaffEmail", staff.Email ?? string.Empty);
            HttpContext.Session.SetString("StaffRole", staff.Position ?? string.Empty);

            return RedirectToAction("Dashboard");
        }



        // GET: Staff/Dashboard
        public IActionResult Dashboard()
        {
            var staffID = HttpContext.Session.GetInt32("StaffID");

            if (staffID == null)
                return RedirectToAction("Login");

            var position = HttpContext.Session.GetString("StaffRole");

            // Show all service requests that match the staff's department (Maintenance or Security)
            var requests = _context.ServiceRequests
                .Where(r => r.Category == position)
                .ToList();

            var pendingRequests = requests.Where(r => r.Status == "Pending").ToList();
            var completedRequests = requests.Where(r => r.Status == "Completed").ToList();

            ViewData["PendingCount"] = pendingRequests.Count;
            ViewData["CompletedCount"] = completedRequests.Count;
            ViewData["PendingRequests"] = pendingRequests;
            ViewData["CompletedRequests"] = completedRequests;

            ViewData["StaffName"] = HttpContext.Session.GetString("StaffName");
            ViewData["Position"] = position;

            return View();
        }





        [HttpPost]
        public IActionResult UpdateRequestStatus(int requestId, string status)
        {
            if (status != "Completed")
            {
                TempData["Error"] = "Invalid status update.";
                return RedirectToAction("Dashboard", "Staff");
            }

            var serviceRequest = _context.ServiceRequests
                .FirstOrDefault(r => r.RequestID == requestId);

            if (serviceRequest != null)
            {
                serviceRequest.Status = status;
                serviceRequest.CompletedAt = DateTime.Now; // Set the completion date

                _context.SaveChanges();

                TempData["Success"] = "Request marked as completed!";
            }

            return RedirectToAction("Dashboard", "Staff");
        }









        public IActionResult Management()
        {
            var staffId = HttpContext.Session.GetInt32("StaffID");

            if (staffId == null)
            {
                return RedirectToAction("Login", "Staff");
            }

            var staff = _context.Staff.FirstOrDefault(s => s.StaffID == staffId);
            if (staff == null)
            {
                ViewData["StaffName"] = "Unknown Staff";
                ViewData["Position"] = "Unknown Position";
            }
            else
            {
                ViewData["StaffName"] = staff.FullName ?? "Unknown Staff";
                ViewData["Position"] = staff.Position ?? "Unknown Position";
            }

            var pendingRequests = _context.ServiceRequests.Where(r => r.Status == "Pending").Include(r => r.Homeowner).ToList();
            var completedRequests = _context.ServiceRequests.Where(r => r.Status == "Completed").Include(r => r.Homeowner).ToList();

            var viewModel = new Dictionary<string, List<ServiceRequest>>()
            {
                { "Pending", pendingRequests },
                { "Completed", completedRequests }
            };

            return PartialView("_ManagementPartial", viewModel);  // Return the partial view instead of full view
        }




        // GET: Staff/Profile
        public IActionResult Profile()
        {
            var staffId = GetCurrentStaffId();
            var staff = _context.Staff.FirstOrDefault(s => s.StaffID == staffId);

            if (staff == null)
                return RedirectToAction("Login");

            return View(staff);
        }

        // GET: Staff/UnauthorizedAccess
        public IActionResult UnauthorizedAccess()
        {
            return View("UnauthorizedAccess");
        }

        // GET: Staff/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Password verification helper
        private static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }

        // Role validation helper
        private bool IsLoggedIn(string requiredRole = "")
        {
            var position = HttpContext.Session.GetString("StaffRole");

            if (string.IsNullOrEmpty(position))
                return false;

            return string.IsNullOrEmpty(requiredRole) || position.ToLower() == requiredRole.ToLower();
        }





        // Staff ID retrieval from session
        private int GetCurrentStaffId()
        {
            var currentUserId = User.Identity?.Name;
            var staff = _context.Staff.FirstOrDefault(s => s.Email == currentUserId);
            return staff?.StaffID ?? 0;
        }
    }
}
