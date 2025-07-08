namespace HOMEOWNER.Models
{
    public class ForumComment
    {
        public int ForumCommentID { get; set; }
        public int ForumPostID { get; set; }
        public int HomeownerID { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ForumPost ForumPost { get; set; } = null!;
        public Homeowner Homeowner { get; set; } = null!;
        public string? MediaUrl { get; set; } // For comment attachments

    }

}
