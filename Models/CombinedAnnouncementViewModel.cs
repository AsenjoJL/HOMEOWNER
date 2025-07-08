namespace HOMEOWNER.Models
{
    public class CombinedAnnouncementViewModel
    {
        public AnnouncementViewModel NewAnnouncement { get; set; } = new();
        public List<Announcement> Announcements { get; set; } = new();
    }

}
