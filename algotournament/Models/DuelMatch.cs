namespace algotournament.Models
{
    public class DuelMatch
    {
        public int Id { get; set; }
        public int DuelRoomId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string? WinnerUserId { get; set; }
        public int DurationMinutes { get; set; }
        public DuelMatchStatus Status { get; set; } = DuelMatchStatus.InProgress;
        public int FinalScorePlayer1 { get; set; }
        public int FinalScorePlayer2 { get; set; }
        public string Player1UserId { get; set; } = string.Empty;
        public string Player2UserId { get; set; } = string.Empty;

        public virtual DuelRoom Room { get; set; } = null!;
        public virtual ApplicationUser? WinnerUser { get; set; }
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }

    public enum DuelMatchStatus
    {
        InProgress,
        Finished,
        Cancelled,
        ForceEnded
    }
}
