using HOMEOWNER.Data;
using HOMEOWNER.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HOMEOWNER.Controllers
{
    public class FacilityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FacilityController> _logger;

        public FacilityController(ApplicationDbContext context, ILogger<FacilityController> logger)
        {
            _context = context;
            _logger = logger;
        }




        // GET: Facility
        public IActionResult Index()
        {
            var facilities = _context.Facilities.ToList();
            return View(facilities);
        }

        // GET: Facility/Add
        public IActionResult Add()
        {
            return View();
        }

        // POST: Facility/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Facility facility, List<IFormFile> ImageFiles)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid input. Please check the form." });
            }

            try
            {
                // Check for duplicate facility name
                var existingFacility = await _context.Facilities
                                                     .FirstOrDefaultAsync(f => f.FacilityName == facility.FacilityName);
                if (existingFacility != null)
                {
                    return Json(new { success = false, message = "Facility name already exists!" });
                }

                // Process image files
                if (ImageFiles != null && ImageFiles.Count > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/facilities");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    List<string> imagePaths = new List<string>();

                    foreach (var imageFile in ImageFiles)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        imagePaths.Add("/images/facilities/" + fileName);
                    }

                    facility.ImageUrl = string.Join(",", imagePaths);
                }

                // Set default availability status
                facility.AvailabilityStatus = "Available";

                // Add facility to the database
                _context.Facilities.Add(facility);
                await _context.SaveChangesAsync();

                // Add facility logic here
                var facilities = _context.Facilities.ToList(); // Or any other data source
                return Json(new { success = true, message = "Facility added successfully.", facilities });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred. Please try again." });
            }
        }






        // GET: Facility/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var facility = await _context.Facilities.FindAsync(id);
            if (facility == null) return NotFound();

            return View(facility);
        }

        // POST: Facility/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Facility facility, IFormFile ImageFile)
        {
            if (id != facility.FacilityID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                        var filePath = Path.Combine("wwwroot/images/facilities", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }
                        facility.ImageUrl = "/images/facilities/" + fileName;
                    }

                    _context.Update(facility);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Facilities.Any(e => e.FacilityID == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(facility);
        }

        // DELETE: Facility/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var facility = await _context.Facilities.FindAsync(id);
            if (facility != null)
            {
                _context.Facilities.Remove(facility);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}