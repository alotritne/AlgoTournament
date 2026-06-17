using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace algotournament.Models
{
    public class Contest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        [Required(ErrorMessage = "Tournament is required.")]
        public int TournamentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsRated { get; set; } = true;
        public ScoringMode ScoringMode { get; set; } = ScoringMode.ACM;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [BindNever]
        public virtual Tournament? Tournament { get; set; }
        [BindNever]
        public virtual ICollection<ContestProblem> ContestProblems { get; set; } = new List<ContestProblem>();
        [BindNever]
        public virtual ICollection<ContestParticipant> Participants { get; set; } = new List<ContestParticipant>();
        [BindNever]
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }

    public enum ScoringMode
    {
        ACM,  // Accepted/Wrong Answer only
        OI    // Partial scoring based on test cases
    }
}
