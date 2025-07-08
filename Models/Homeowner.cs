namespace HOMEOWNER.Models
{
    public class Homeowner
    {

        public int HomeownerID { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string ContactNumber { get; set; } = string.Empty; // ✅ Add this if missing
        public string? Address { get; set; }
        public string? Role { get; set; } = "Homeowner"; // Default role
        public string? BlockLotNumber { get; set; }
        public string? PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int AdminID { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
