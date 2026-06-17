using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace algotournament.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; } = string.Empty; // Markdown
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsGlobal { get; set; } = true;
        public int? ContestId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [BindNever]
        public virtual ApplicationUser? Creator { get; set; }
        [BindNever]
        public virtual Contest? Contest { get; set; }
    }
}
