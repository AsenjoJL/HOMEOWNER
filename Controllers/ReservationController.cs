using HOMEOWNER.Data;
using HOMEOWNER.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HOMEOWNER.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }


        private void ExpireOldReservations()
        {
            var now = DateTime.Now;

            var approvedReservations = _context.Reservations
                .Where(r => r.Status == "Approved")
                .ToList();

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


        public IActionResult Index()
        {
            var homeownerId = HttpContext.Session.GetInt32("HomeownerID");
            if (homeownerId == null) return RedirectToAction("Login", "Account");


            ExpireOldReservations(); // 🔧 Call it here



            // Fetch available facilities
            List<Facility> facilities = _context.Facilities
                .Where(f => f.AvailabilityStatus == "Available")
                .ToList();



            // For admin or homeowner, show only current non-expired reservations
            var reservations = _context.Reservations
                .Where(r => r.Status != "Expired") // Filter out expired reservations
                .Include(r => r.Facility)
                .OrderByDescending(r => r.ReservationDate)
                .ToList();


            // ✅ Count reservations only for the logged-in homeowner
            int activityCount = _context.Reservations
                .Where(r => r.HomeownerID == homeownerId && r.Status == "Approved")
                .Count();

            ViewBag.ActivityCount = activityCount; // Send count to the view

            return View(facilities);
        }

        [HttpPost]
        public async Task<IActionResult> ReserveFacility(int facilityId, TimeSpan startTime, TimeSpan endTime, string purpose)
        {
            var homeownerId = HttpContext.Session.GetInt32("HomeownerID");
            if (homeownerId == null)
                return RedirectToAction("Login", "Account");

            if (startTime >= endTime)
            {
                TempData["Error"] = "End time must be later than start time.";
                return RedirectToAction("Index");
            }

            var homeownerExists = await _context.Homeowners.AnyAsync(h => h.HomeownerID == homeownerId);
            var facilityExists = await _context.Facilities.AnyAsync(f => f.FacilityID == facilityId);

            if (!homeownerExists || !facilityExists)
            {
                TempData["Error"] = "Invalid Homeowner or Facility ID.";
                return RedirectToAction("Index");
            }

            var isConflict = await _context.Reservations.AnyAsync(r =>
                r.FacilityID == facilityId &&
                r.Status == "Approved" &&
                (r.StartTime < endTime && r.EndTime > startTime));

            if (isConflict)
            {
                TempData["Error"] = "This facility is already reserved for the selected time slot.";
                return RedirectToAction("Index");
            }

            var reservation = new Reservation
            {
                HomeownerID = homeownerId.Value,
                FacilityID = facilityId,
                StartTime = startTime,
                EndTime = endTime,
                Purpose = string.IsNullOrEmpty(purpose) ? "No Purpose Provided" : purpose,
                ReservationDate = DateTime.Now,
                Status = "Approved", // 👈 Auto-approved!
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            try
            {
                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Facility reserved successfully!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database Error: {ex.InnerException?.Message}");
                TempData["Error"] = $"Database Error: {ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }



        public async Task<IActionResult> History()
        {
            var homeownerId = HttpContext.Session.GetInt32("HomeownerID");
            if (homeownerId == null)
                return RedirectToAction("Login", "Account");

            var reservations = await _context.Reservations
                .Include(r => r.Facility)
                .Where(r => r.HomeownerID == homeownerId && (r.Status == "Expired" || r.Status == "Canceled"))
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            return View(reservations); // Show expired reservations for the homeowner
        }


        public async Task<IActionResult> ViewExpiredHistory()
        {
            // Fetch expired reservations for admin
            var expiredReservations = await _context.Reservations
                .Include(r => r.Facility)
                .Include(r => r.Homeowner)
                .Where(r => r.Status == "Expired")
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            return View(expiredReservations); // Return expired reservations view
        }







    }

}