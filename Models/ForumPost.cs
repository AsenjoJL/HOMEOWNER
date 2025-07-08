namespace HOMEOWNER.Models
{
    public class ForumPost
    {
        public int ForumPostID { get; set; }
        public int HomeownerID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Homeowner Homeowner { get; set; } = null!;
        public List<ForumComment> Comments { get; set; } = new();
        public ICollection<Reaction> Reactions { get; set; } // Add this line
        public string? MediaUrl { get; set; } // For images/videos
        public string? MediaType { get; set; } // "image", "video", "audio"
        public string? MusicUrl { get; set; } // For music files
        public string? MusicTitle { get; set; } // Optional music title

    }
}

