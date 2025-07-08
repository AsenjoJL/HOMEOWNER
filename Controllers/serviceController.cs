using HOMEOWNER.Data;
using HOMEOWNER.Models;
using Microsoft.AspNetCore.Mvc;

public class ServiceController : Controller
{
    private readonly ApplicationDbContext _context;

    public ServiceController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Service/SubmitRequest - Displays the form
    public IActionResult SubmitRequest()
    {
        return View();
    }

    // POST: Service/SubmitRequest - Handles the form submission
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitRequest(ServiceRequest request)
    {
        if (ModelState.IsValid)
        {
            request.HomeownerID = GetCurrentHomeownerId();
            request.CreatedAt = DateTime.Now;
            request.Status = "Pending";

            // Auto-assign staff based on the category (Maintenance or Security)
            var staff = _context.Staff
           .Where(s =>
               !string.IsNullOrEmpty(s.Position) &&
               string.Equals(s.Position, request.Category ?? string.Empty, StringComparison.OrdinalIgnoreCase)
           )
           .OrderBy(s => Guid.NewGuid()) // Random staff assignment
           .FirstOrDefault();


            if (staff != null)
            {
                request.AssignedStaffID = staff.StaffID;
            }

            _context.ServiceRequests.Add(request);
            _context.SaveChanges();

            TempData["Success"] = "Your service request has been submitted and routed.";
            return RedirectToAction("SubmitRequest");
        }

        TempData["Error"] = "There was an error submitting your request.";
        return View(request);
    }



    // Get current logged-in Homeowner ID (this should be based on your authentication system)
    private int GetCurrentHomeownerId()
    {
        // Assuming you have a mechanism to fetch the current logged-in user
        // Replace this part with how you are getting the logged-in user, e.g., from a session or a claim

        var currentUserId = User.Identity?.Name; // This can be the email or username of the logged-in user

        // Now fetch the HomeownerID based on the logged-in user
        var homeowner = _context.Homeowners.FirstOrDefault(h => h.Email == currentUserId); // Adjust if you're using something other than email
        if (homeowner == null)
        {
            throw new Exception("Homeowner not found.");
        }

        return homeowner.HomeownerID; // Return the HomeownerID from the Homeowners table
    }


    public IActionResult ViewRequests()
    {
        // Retrieve service requests for the logged-in homeowner (based on HomeownerID)
        var homeownerId = GetCurrentHomeownerId();  // Replace with actual logic to get the logged-in HomeownerID
        var serviceRequests = _context.ServiceRequests
            .Where(r => r.HomeownerID == homeownerId)
            .ToList();

        return View(serviceRequests);  // Pass the requests to the view
    }



}

