namespace algotournament.Models
{
    public class ContestProblem
    {
        public int Id { get; set; }
        public int ContestId { get; set; }
        public int ProblemId { get; set; }
        public string ProblemLetter { get; set; } = string.Empty; // A, B, C, etc.
        public int Order { get; set; } = 0;
        public int Points { get; set; } = 100;
        
        // Navigation properties
        public virtual Contest Contest { get; set; } = null!;
        public virtual Problem Problem { get; set; } = null!;
    }
}