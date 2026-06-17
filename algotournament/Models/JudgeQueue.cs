namespace algotournament.Models
{
    public class JudgeQueue
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public JudgeQueueStatus Status { get; set; } = JudgeQueueStatus.Pending;
        public int Priority { get; set; } = 0;
        public DateTime QueuedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? AssignedWorker { get; set; }
        public int RetryCount { get; set; } = 0;
        public string? ErrorMessage { get; set; }
        
        // Navigation properties
        public virtual Submission Submission { get; set; } = null!;
    }
    
    public enum JudgeQueueStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled
    }
}