namespace algotournament.Models
{
    public class Discussion
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public int? ProblemId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int ReplyCount { get; set; } = 0;
        public int ViewCount { get; set; } = 0;
        
        // Navigation properties
        public virtual ApplicationUser Author { get; set; } = null!;
        public virtual Problem? Problem { get; set; }
        public virtual ICollection<DiscussionReply> Replies { get; set; } = new List<DiscussionReply>();
    }
}
