using Microsoft.AspNetCore.Identity;

namespace algotournament.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Handle { get; set; } = string.Empty;
        public int Rating { get; set; } = 1200;
        public int ContestsParticipated { get; set; } = 0;
        public int ProblemsSolved { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsBanned { get; set; } = false;
        public string? BanReason { get; set; }
        
        // Navigation properties
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
        public virtual ICollection<ContestParticipant> ContestParticipations { get; set; } = new List<ContestParticipant>();
    }
}