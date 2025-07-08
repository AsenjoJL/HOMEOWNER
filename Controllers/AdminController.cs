using HOMEOWNER.Data;
using HOMEOWNER.Models;
using HOMEOWNER.Models.ViewModels;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Cryptography;




namespace HOMEOWNER.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;


        public AdminController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;


        }

        public IActionResult Dashboard()
        {
            var homeownerCount = _context.Homeowners.Count(u => u.Role == "Homeowner");
            var staffCount = _context.Staff.Count(u => u.Position == "Maintenance" || u.Position == "Security");
            var reservationCount = _context.Reservations.Count(r => r.Status == "Approved");


            ViewBag.HomeownerCount = homeownerCount;
            ViewBag.StaffCount = staffCount;
            ViewBag.ReservationCount = reservationCount;
            // ✅ Fetch homeowners from database
            var homeowners = _context.Homeowners.ToList();

            if (homeowners == null)
            {
                homeowners = new List<Homeowner>();  // ✅ Ensure it is not null
            }

            return View(homeowners); // ✅ Pass homeowners list to the view

        }





        public IActionResult ManageOwners()
        {
            var homeowners = _context.Homeowners.ToList();

            if (homeowners == null)
            {
                homeowners = new List<Homeowner>(); // Ensure it's not null
            }

            return PartialView("_ManageOwners", homeowners); // ✅ Use PartialView for partials
        }


        public IActionResult AddOwner()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOwner(Homeowner owner)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data provided." });
            }

            // Check for duplicate email
            if (await _context.Homeowners.AnyAsync(h => h.Email == owner.Email))
            {
                return Json(new { success = false, message = "This email is already registered." });
            }

            // Check for duplicate BlockLotNumber
            if (await _context.Homeowners.AnyAsync(h => h.BlockLotNumber == owner.BlockLotNumber))
            {
                return Json(new { success = false, message = "This Block & Lot Number is already taken." });
            }

            // Hash the password before storing it
            owner.PasswordHash = HashPassword(owner.PasswordHash);

            // Assign default values
            owner.AdminID = 1; // Set dynamically based on logged-in admin
            owner.Role = "Homeowner";
            owner.CreatedAt = DateTime.Now;

            _context.Homeowners.Add(owner);
            await _context.SaveChangesAsync();

            //  Return JSON data instead of HTML
            return Json(new
            {
                success = true,
                homeowner = new
                {
                    id = owner.HomeownerID,
                    fullName = owner.FullName,
                    email = owner.Email,
                    address = owner.Address,
                    contactNumber = owner.ContactNumber,
                    blockLotNumber = owner.BlockLotNumber,
                    role = owner.Role
                }
            });
        }



        public IActionResult ManageStaff()
        {
            var staffList = _context.Staff.ToList();
            return View(staffList);
        }

        public IActionResult LoadStaffList()
        {
            var staffList = _context.Staff.ToList();
            return PartialView("_StaffList", staffList); // ✅ Ensure this file exists in Views/Admin/
        }

        [HttpPost]
        public IActionResult AddStaff(Staff staff)
        {
            if (ModelState.IsValid)
            {
                // ✅ Ensure default values if any field is missing
                staff.FullName = staff.FullName ?? "Unknown";
                staff.Email = staff.Email ?? "No Email";
                staff.PhoneNumber = staff.PhoneNumber ?? "No Phone";
                staff.Position = staff.Position ?? "No Position";


                // Hash the password before storing it
                staff.PasswordHash = HashPassword(staff.PasswordHash);

                _context.Staff.Add(staff);
                _context.SaveChanges(); // ✅ Save to DB

                var savedStaff = _context.Staff.OrderByDescending(s => s.StaffID).FirstOrDefault();

                return Json(new { success = true, staff = savedStaff });
            }
            return Json(new { success = false, message = "❌ Failed to add staff." });
        }





        public IActionResult EditStaff(int id)
        {
            var staff = _context.Staff.Find(id);
            if (staff == null)
            {
                return NotFound();
            }
            return View(staff);
        }

        [HttpPost]
        public async Task<IActionResult> EditStaff(Staff model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var staff = await _context.Staff.FindAsync(model.StaffID);
            if (staff == null)
            {
                return NotFound();
            }

            staff.FullName = model.FullName;
            staff.Email = model.Email;
            staff.PhoneNumber = model.PhoneNumber;
            staff.Position = model.Position;

            _context.Staff.Update(staff);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageStaff");
        }








        private static string HashPassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password), "Password cannot be empty or null.");

            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32
            );

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public async Task<IActionResult> FacilitiesAndReservations()
        {
            var facilities = await _context.Facilities.ToListAsync();
            var reservations = await _context.Reservations
               .Include(r => r.Facility)
              .Include(r => r.Homeowner)
              .ToListAsync();

            var model = new Tuple<IEnumerable<Facility>, IEnumerable<Reservation>>(facilities, reservations);
            return View(model);
        }





        // POST: Delete Facility
        [HttpPost]
        public async Task<IActionResult> DeleteFacility(int facilityId)
        {
            var facility = await _context.Facilities.FindAsync(facilityId);
            if (facility == null)
            {
                return NotFound();
            }

            _context.Facilities.Remove(facility);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(FacilitiesAndReservations));
        }

        public IActionResult ReservationManagement()
        {
            IEnumerable<Facility> facilities = _context.Facilities.ToList();
            IEnumerable<Reservation> reservations = _context.Reservations
                .Include(r => r.Facility)
                .Include(r => r.Homeowner) // ✅ Pull in FullName
                .ToList();

            var model = Tuple.Create(facilities, reservations);
            return View(model);
        }


        private void ExpireOldReservations()
        {
            var now = DateTime.Now;

            // Step 1: Pull approved reservations into memory
            var approvedReservations = _context.Reservations
                .Where(r => r.Status == "Approved")
                .ToList();

            // Step 2: Expire those that are already finished
            foreach (var reservation in approvedReservations)
            {
                var endDateTime = reservation.ReservationDate.Date + reservation.EndTime;
                if (endDateTime <= now)
                {
                    reservation.Status = "Expired";
                    reservation.UpdatedAt = now;
                }
            }

            _context.SaveChanges();
        }










        // Manage service requests (Admin View)
        public IActionResult ManageServiceRequests(string statusFilter = "All")
        {
            var serviceRequests = _context.ServiceRequests.AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                serviceRequests = serviceRequests.Where(r => r.Status == statusFilter);
            }

            var viewModel = new SubmitRequestViewModel
            {
                ServiceRequests = serviceRequests.ToList(),
                StaffList = _context.Staff.ToList()
            };

            return View(viewModel);
        }

        // Helper method to return a list of categories
        private List<string> GetEventCategories()
        {
            return new List<string> { "General", "Meeting", "Conference", "Workshop", "Webinar", "Party", "Training" };
        }

        // Event List View
        [HttpGet]
        public IActionResult EventCalendar()
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

            return View(events);
        }

        [HttpGet]
        public IActionResult AddEvent()
        {
            ViewBag.Categories = GetEventCategories();
            return PartialView("_AddEditEventPartial", new EventModel());
        }

        // Handle Add Event Submission (POST)
        [HttpPost]
        public IActionResult AddEvent(EventModel model)
        {
            ViewBag.Categories = GetEventCategories();

            if (!ModelState.IsValid || model.EventDate < new DateTime(1753, 1, 1))
            {
                if (model.EventDate < new DateTime(1753, 1, 1))
                {
                    ModelState.AddModelError("EventDate", "Please select a valid event date.");
                }

                return PartialView("_AddEditEventPartial", model); // return partial with errors
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("HOME_DB")))
                {
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO Events (Title, Description, EventDate, Category, CreatedBy, Location)
                                   VALUES (@Title, @Description, @EventDate, @Category, @CreatedBy, @Location)", conn);

                    cmd.Parameters.AddWithValue("@Title", model.Title);
                    cmd.Parameters.AddWithValue("@Description", model.Description ?? string.Empty);
                    cmd.Parameters.AddWithValue("@EventDate", model.EventDate);
                    cmd.Parameters.AddWithValue("@Category", model.Category ?? "General");
                    cmd.Parameters.AddWithValue("@CreatedBy", GetCurrentAdminID());
                    cmd.Parameters.AddWithValue("@Location", model.Location ?? "Not set");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                return Json(new { success = true, message = "Event added successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                ModelState.AddModelError(string.Empty, "An error occurred while saving.");
                return PartialView("_AddEditEventPartial", model);
            }
        }



        // GET: Edit Event
        [HttpGet]
        public IActionResult EditEvent(int id)
        {
            EventModel eventModel = new EventModel(); // Initialize with a new instance

            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("HOME_DB")))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Events WHERE EventID = @EventID", conn);
                cmd.Parameters.AddWithValue("@EventID", id);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    eventModel = new EventModel
                    {
                        EventID = (int)reader["EventID"],
                        Title = reader["Title"].ToString(),
                        Description = reader["Description"].ToString(),
                        EventDate = (DateTime)reader["EventDate"],
                        Category = reader["Category"].ToString(),
                        Location = reader["Location"]?.ToString()
                    };
                }
            }

            if (eventModel == null)
                return NotFound();

            ViewBag.Categories = GetEventCategories();
            return PartialView("_AddEditEventPartial", eventModel);
        }

        // POST: Edit Event
        [HttpPost]
        public IActionResult EditEvent(EventModel model)
        {
            ViewBag.Categories = GetEventCategories();

            if (!ModelState.IsValid || model.EventDate < new DateTime(1753, 1, 1))
            {
                if (model.EventDate < new DateTime(1753, 1, 1))
                {
                    ModelState.AddModelError("EventDate", "Please select a valid event date.");
                }

                return PartialView("_AddEditEventPartial", model); // Returning partial view here for AJAX to reload
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("HOME_DB")))
                {
                    SqlCommand cmd = new SqlCommand(@"UPDATE Events SET Title = @Title, Description = @Description,
                                              EventDate = @EventDate, Category = @Category, Location = @Location
                                              WHERE EventID = @EventID", conn);

                    cmd.Parameters.AddWithValue("@Title", model.Title);
                    cmd.Parameters.AddWithValue("@Description", model.Description ?? string.Empty);
                    cmd.Parameters.AddWithValue("@EventDate", model.EventDate);
                    cmd.Parameters.AddWithValue("@Category", model.Category ?? "General");
                    cmd.Parameters.AddWithValue("@Location", model.Location ?? "Not set");
                    cmd.Parameters.AddWithValue("@EventID", model.EventID);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                return Json(new { success = true, message = "Event updated successfully!" });  // Return success to reload event list on success
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(string.Empty, "Database error occurred while updating the event.");
                Console.WriteLine("SQL Error: " + ex.Message);
                return Json(new { success = false, message = "Error updating the event. Please try again." });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Unexpected error. Please try again.");
                Console.WriteLine("General Error: " + ex.Message);
                return Json(new { success = false, message = "Unexpected error. Please try again." });
            }
        }


        // POST: Delete Event
        [HttpPost]
        public IActionResult DeleteEvent(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("HOME_DB")))
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM Events WHERE EventID = @EventID", conn);
                    cmd.Parameters.AddWithValue("@EventID", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                return Json(new { success = true, message = "Event deleted successfully!" });  // Return success for AJAX
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting event: " + ex.Message);
                return Json(new { success = false, message = "Error deleting event. Please try again." });
            }
        }


        // Helper: Get Logged-in Admin ID
        private int GetCurrentAdminID()
        {
            // TODO: Replace with session or auth logic
            return 1;
        }

        // GET: Show the form for creating an announcement and the list
        public IActionResult AnnouncementList()
        {
            var model = new CombinedAnnouncementViewModel
            {
                Announcements = _context.Announcements.OrderByDescending(a => a.PostedAt).ToList()
            };

            return View(model);
        }

        // POST: Create a new announcement (AJAX modal support)
        [HttpPost]
        public async Task<IActionResult> AnnouncementList(AnnouncementViewModel model)
        {
            if (ModelState.IsValid)
            {
                var announcement = new Announcement
                {
                    Title = model.Title,
                    Content = model.Content,
                    PostedAt = DateTime.Now,
                    IsUrgent = model.IsUrgent
                };

                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();


                var partialModel = new CombinedAnnouncementViewModel
                {
                    Announcements = _context.Announcements.OrderByDescending(a => a.PostedAt).ToList()
                };

                // Return only the updated list for AJAX calls
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_AnnouncementListPartial", partialModel);
                }

                TempData["Success"] = "Announcement posted successfully!";
                return RedirectToAction("AnnouncementList");
            }

            // Return validation errors
            var viewModel = new CombinedAnnouncementViewModel
            {
                NewAnnouncement = model,
                Announcements = _context.Announcements.OrderByDescending(a => a.PostedAt).ToList()
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                Response.StatusCode = 400;
                return PartialView("_AnnouncementListPartial", viewModel);
            }

            return View(viewModel);
        }


        public IActionResult Analytics()
        {
            var facilities = _context.Facilities.ToList();
            var reservations = _context.Reservations
                .Include(r => r.Facility)
                .Include(r => r.Homeowner)
                .ToList();

            var model = new Tuple<IEnumerable<Facility>, IEnumerable<Reservation>>(facilities, reservations);
            return View(model);
        }

    }
}
