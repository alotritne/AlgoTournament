namespace algotournament.Models
{
    public class SubmissionResult
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public int TestCaseId { get; set; }
        public SubmissionStatus Status { get; set; }
        public int ExecutionTimeMs { get; set; } = 0;
        public int MemoryUsedKB { get; set; } = 0;
        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsPublic { get; set; } = false;
        
        // Navigation properties
        public virtual Submission Submission { get; set; } = null!;
        public virtual TestCase TestCase { get; set; } = null!;
    }
}