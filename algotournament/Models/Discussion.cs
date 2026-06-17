using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace algotournament.Models
{
    public class Discussion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public int? ProblemId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int ReplyCount { get; set; } = 0;
        public int ViewCount { get; set; } = 0;

        // Navigation properties
        [BindNever]
        public virtual ApplicationUser? Author { get; set; }
        [BindNever]
        public virtual Problem? Problem { get; set; }
        [BindNever]
        public virtual ICollection<DiscussionReply> Replies { get; set; } = new List<DiscussionReply>();
    }
}
