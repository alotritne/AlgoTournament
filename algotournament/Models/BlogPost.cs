namespace algotournament.Models
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsPublished { get; set; } = true;

        // Navigation properties
        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public virtual ApplicationUser Author { get; set; } = null!;
    }
}
