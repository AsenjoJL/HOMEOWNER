namespace HOMEOWNER.Models
{
    public class Reaction
    {
        public int ReactionID { get; set; }
        public string ReactionType { get; set; } // "like", "love", "laugh", "insightful"
        public DateTime CreatedAt { get; set; }
        public int ForumPostID { get; set; }
        public ForumPost ForumPost { get; set; }
        public int HomeownerID { get; set; }
        public Homeowner Homeowner { get; set; }
    }
}
