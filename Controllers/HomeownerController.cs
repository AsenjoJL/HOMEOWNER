using HOMEOWNER.Data;
using HOMEOWNER.Models;
using HOMEOWNER.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;

namespace HOMEOWNER.Controllers
{


    public class HomeownerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string? _connectionString;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _config;


        public HomeownerController(ApplicationDbContext context, IConfiguration configuration, IWebHostEnvironment hostingEnvironment, IConfiguration config)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("HOME_DB");
            _hostingEnvironment = hostingEnvironment;
            _config = config;

        }

        public async Task<IActionResult> Dashboard()
        {
            int homeownerId = GetCurrentHomeownerId();

            var profileImage = await _context.HomeownerProfileImages
                .Where(h => h.HomeownerID == homeownerId)
                .Select(h => h.ImagePath)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(profileImage))
            {
                profileImage = "/uploads/profile_pictures/default-profile.jpg"; // Default profile picture
            }

            ViewData["ProfileImage"] = profileImage;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfileImage(IFormFile profileImage)
        {
            if (profileImage == null || profileImage.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            int homeownerId = GetCurrentHomeownerId();
            if (homeownerId == 0)
            {
                return Unauthorized("User not logged in.");
            }

            var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads/profile_pictures");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{homeownerId}_{DateTime.Now.Ticks}{Path.GetExtension(profileImage.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }

            var imagePath = $"/uploads/profile_pictures/{fileName}";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("sp_UpdateHomeownerProfileImage", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@HomeownerID", homeownerId);
                command.Parameters.AddWithValue("@ImagePath", imagePath);

                var result = await command.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    return Ok(new { message = "Profile image updated successfully.", imagePath });
                }
                else
                {
                    return BadRequest("You have reached the maximum profile updates for today.");
                }
            }
        }
        private int GetCurrentHomeownerId()
        {
            var homeownerId = User.FindFirst("HomeownerID")?.Value ?? "0";
            return int.TryParse(homeownerId, out int id) ? id : 0;
        }

        // Display the Submit Request form
        [HttpGet]
        public IActionResult SubmitRequest()
        {
            // Check if user is logged in by verifying the session or claims
            var homeownerId = HttpContext.Session.GetInt32("HomeownerID");

            if (homeownerId == null)
            {
                return RedirectToAction("Login", "Account");  // Redirect to login if not logged in
            }

            // Populate the model with request data
            var viewModel = new SubmitRequestViewModel
            {
                NewRequest = new ServiceRequest(),
                ServiceRequests = _context.ServiceRequests
                    .Where(r => r.HomeownerID == homeownerId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList(),
                StaffList = _context.Staff.ToList(),
                HomeownerId = homeownerId.Value,
                HomeownerName = _context.Homeowners
                    .Where(h => h.HomeownerID == homeownerId)
                    .Select(h => h.FullName)
                    .FirstOrDefault()
            };

            return View("~/Views/Service/SubmitRequest.cshtml", viewModel);
        }

        [HttpPost]
        public IActionResult SubmitRequest(SubmitRequestViewModel model)
        {
            var homeownerId = HttpContext.Session.GetInt32("HomeownerID");

            if (ModelState.IsValid && model.NewRequest != null)
            {
                if (homeownerId != null)
                {
                    model.NewRequest.HomeownerID = homeownerId.Value;
                    model.NewRequest.CreatedAt = DateTime.Now;

                    if (string.IsNullOrEmpty(model.NewRequest.Status))
                    {
                        model.NewRequest.Status = "Pending";
                    }

                    if (model.NewRequest.Category == "Security")
                    {
                        model.NewRequest.AssignedStaffID = _context.Staff
                            .Where(s => s.Position == "Security")
                            .Select(s => s.StaffID)
                            .FirstOrDefault();
                    }
                    else if (model.NewRequest.Category == "Maintenance")
                    {
                        model.NewRequest.AssignedStaffID = _context.Staff
                            .Where(s => s.Position == "Maintenance")
                            .Select(s => s.StaffID)
                            .FirstOrDefault();
                    }

                    _context.ServiceRequests.Add(model.NewRequest);
                    _context.SaveChanges();

                    TempData["Success"] = "Request submitted successfully!";
                    return RedirectToAction("SubmitRequest");
                }
                else
                {
                    ModelState.AddModelError("", "Homeowner ID is missing or invalid.");
                }
            }

            if (homeownerId != null)
            {
                model.ServiceRequests = _context.ServiceRequests
                    .Where(r => r.HomeownerID == homeownerId.Value && r.Status != "Cancelled")
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();
                model.StaffList = _context.Staff.ToList();
                model.HomeownerId = homeownerId.Value;
                model.HomeownerName = _context.Homeowners
                    .Where(h => h.HomeownerID == homeownerId.Value)
                    .Select(h => h.FullName)
                    .FirstOrDefault();
            }

            return View("~/Views/Service/SubmitRequest.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> CancelRequest(int requestId)
        {
            var request = await _context.ServiceRequests.FindAsync(requestId);

            if (request == null)
            {
                return Json(new { success = false, message = "Request not found." });
            }

            if (request.Status != "Pending")
            {
                return Json(new { success = false, message = "Only pending requests can be canceled." });
            }

            // Delete the request from the database
            _context.ServiceRequests.Remove(request);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }







        // View the submitted service requests for the logged-in homeowner
        public IActionResult ViewRequest()
        {
            var homeownerId = GetCurrentHomeownerId();
            var serviceRequests = _context.ServiceRequests
                .Where(r => r.HomeownerID == homeownerId)
                .ToList();

            return View(serviceRequests); // This will show the requests along with their status
        }

        [HttpGet]
        public IActionResult Calendar()
        {
            List<EventModel> events = new List<EventModel>();

            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("HOME_DB")))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Events ORDER BY EventDate", conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    events.Add(new EventModel
                    {
                        EventID = (int)reader["EventID"],
                        Title = reader["Title"].ToString(),
                        Description = reader["Description"].ToString(),
                        EventDate = (DateTime)reader["EventDate"],
                        Category = reader["Category"].ToString(),
                        Location = reader["Location"]?.ToString() ?? "-"
                    });
                }
            }

            // Serialize events and pass to the view
            var serializedEvents = JsonConvert.SerializeObject(events.Select(e => new
            {
                title = e.Title,
                start = e.EventDate.ToString("yyyy-MM-dd"),
                description = e.Description,
                location = e.Location,
                category = e.Category
            }));

            ViewBag.SerializedEvents = serializedEvents; // Pass serialized events to the view
            return View();
        }








    }
}
