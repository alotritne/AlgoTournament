namespace algotournament.Models
{
    public class Contest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TournamentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsRated { get; set; } = true;
        public ScoringMode ScoringMode { get; set; } = ScoringMode.ACM;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Tournament Tournament { get; set; } = null!;
        public virtual ICollection<ContestProblem> ContestProblems { get; set; } = new List<ContestProblem>();
        public virtual ICollection<ContestParticipant> Participants { get; set; } = new List<ContestParticipant>();
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
    
    public enum ScoringMode
    {
        ACM,  // Accepted/Wrong Answer only
        OI   // Partial scoring based on test cases
    }
}