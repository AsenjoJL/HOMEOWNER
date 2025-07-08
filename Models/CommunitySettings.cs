namespace HOMEOWNER.Models
{
    public class CommunitySettings
    {
        public int CommunitySettingsID { get; set; }
        public string BackgroundImageUrl { get; set; } = "/images/default-forum-bg.jpg";
        public string? CustomCSS { get; set; }
        public string? FeaturedMusicUrl { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
