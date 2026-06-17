namespace algotournament.Models
{
    public class Ranking
    {
        public int Id { get; set; }
        public int ContestId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Rank { get; set; }
        public int TotalScore { get; set; }
        public int ProblemsSolved { get; set; }
        public int PenaltyTime { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Contest Contest { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}