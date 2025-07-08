using HOMEOWNER.Data;
using HOMEOWNER.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HOMEOWNER.Controllers
{
    public class HomeownerProfileImageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeownerProfileImageController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ✅ Upload Profile Image (Limit: 3 times per day)
        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var homeownerId = GetCurrentHomeownerId();
            var existingImage = await _context.HomeownerProfileImages.FirstOrDefaultAsync(h => h.HomeownerID == homeownerId);
            DateTime today = DateTime.UtcNow.Date;

            if (existingImage != null)
            {
                // ✅ Reset count if it's a new day
                if (existingImage.LastUpdatedDate < today)
                {
                    existingImage.ChangeCount = 0;
                    existingImage.LastUpdatedDate = today;
                }

                // ✅ Check if limit (3 changes per day) is reached
                if (existingImage.ChangeCount >= 3)
                {
                    return BadRequest("You can only change your profile picture 3 times per day.");
                }
            }

            // ✅ Save the new image
            string fileName = $"homeowner_{homeownerId}{Path.GetExtension(file.FileName)}";
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile_pictures");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            string filePath = Path.Combine(uploadPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            string imagePath = $"/uploads/profile_pictures/{fileName}";

            if (existingImage != null)
            {
                existingImage.ImagePath = imagePath;
                existingImage.UploadedAt = DateTime.Now;
                existingImage.ChangeCount += 1; // ✅ Increase count
                _context.HomeownerProfileImages.Update(existingImage);
            }
            else
            {
                _context.HomeownerProfileImages.Add(new HomeownerProfileImage
                {
                    HomeownerID = homeownerId,
                    ImagePath = imagePath,
                    ChangeCount = 1, // ✅ First change
                    LastUpdatedDate = today
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { imagePath });
        }

        private int GetCurrentHomeownerId()
        {
            return int.Parse(User.FindFirst("HomeownerID")?.Value ?? "0");
        }
    }
}
