namespace algotournament.Models
{
    public class DiscussionReply
    {
        public int Id { get; set; }
        public int DiscussionId { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Discussion Discussion { get; set; } = null!;
        public virtual ApplicationUser Author { get; set; } = null!;
    }
}
