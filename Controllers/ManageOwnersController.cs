using HOMEOWNER.Data;
using HOMEOWNER.Models;
using Microsoft.AspNetCore.Mvc;

namespace HOMEOWNER.Controllers
{
    public class ManageOwnersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManageOwnersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var homeowners = _context.Homeowners.ToList(); // List<Homeowner>
            return View("~/Views/Admin/Manageowners.cshtml", homeowners);
        }

        [HttpPost]
        public IActionResult AddOwner(Homeowner homeowner)
        {
            Console.WriteLine("Received Data:");
            Console.WriteLine($"Full Name: {homeowner.FullName}");
            Console.WriteLine($"Email: {homeowner.Email}");
            Console.WriteLine($"Role: {homeowner.Role}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model Validation Failed!");
                return View("~/Views/Admin/Manageowners.cshtml", _context.Homeowners.ToList());
            }

            try
            {
                _context.Homeowners.Add(homeowner);
                _context.SaveChanges();
                Console.WriteLine("Saved Successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database Error: " + ex.Message);
            }

            return RedirectToAction("Index");
        }
    }
}
