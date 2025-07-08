namespace HOMEOWNER.Models
{
    public class Announcement
    {
        public int AnnouncementID { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime PostedAt { get; set; }
        public bool IsUrgent { get; set; } // Flag to mark as urgent
    }

}
