namespace algotournament.Models
{
    public class ContestParticipant
    {
        public int Id { get; set; }
        public int ContestId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public int TotalScore { get; set; } = 0;
        public int PenaltyTime { get; set; } = 0;
        public int ProblemsSolved { get; set; } = 0;
        
        // Navigation properties
        public virtual Contest Contest { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}