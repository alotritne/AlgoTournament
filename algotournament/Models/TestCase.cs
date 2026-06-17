namespace algotournament.Models
{
    public class TestCase
    {
        public int Id { get; set; }
        public int ProblemId { get; set; }
        public string Input { get; set; } = string.Empty;
        public string ExpectedOutput { get; set; } = string.Empty;
        public int Points { get; set; } = 10;
        public bool IsSample { get; set; } = false;
        public int Order { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Problem Problem { get; set; } = null!;
    }
}