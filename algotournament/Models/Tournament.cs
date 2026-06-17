using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace algotournament.Models
{
    public class Tournament
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        [Required(ErrorMessage = "Season is required.")]
        public int SeasonId { get; set; }
        public bool IsPrivate { get; set; } = false;
        public string? AccessCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [BindNever]
        public virtual Season? Season { get; set; }
        [BindNever]
        public virtual ICollection<Contest> Contests { get; set; } = new List<Contest>();
    }
}