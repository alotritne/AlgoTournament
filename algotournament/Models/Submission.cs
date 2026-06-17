namespace algotournament.Models
{
    public class Submission
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ProblemId { get; set; }
        public int? ContestId { get; set; }
        public int? DuelMatchId { get; set; }
        public string SourceCode { get; set; } = string.Empty;
        public ProgrammingLanguage Language { get; set; } = ProgrammingLanguage.Cpp17;
        public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
        public int Score { get; set; } = 0;
        public int ExecutionTimeMs { get; set; } = 0;
        public int MemoryUsedKB { get; set; } = 0;
        public string? CompileError { get; set; }
        public string? RuntimeError { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? JudgedAt { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Problem Problem { get; set; } = null!;
        public virtual Contest? Contest { get; set; }
        public virtual DuelMatch? DuelMatch { get; set; }
        public virtual ICollection<SubmissionResult> Results { get; set; } = new List<SubmissionResult>();
    }
    
    public enum ProgrammingLanguage
    {
        Cpp17,
        Cpp20
    }
    
    public enum SubmissionStatus
    {
        Pending,
        Queueing,
        Compiling,
        Running,
        Accepted,
        WrongAnswer,
        RuntimeError,
        CompilationError,
        TimeLimitExceeded,
        MemoryLimitExceeded,
        PresentationError
    }
}