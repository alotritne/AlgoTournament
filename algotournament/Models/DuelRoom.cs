namespace algotournament.Models
{
    public class DuelRoom
    {
        public int Id { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public int ProblemId { get; set; }
        public string HostUserId { get; set; } = string.Empty;
        public int MaxPlayers { get; set; } = 2;
        public int DurationMinutes { get; set; } = 15;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DuelRoomStatus Status { get; set; } = DuelRoomStatus.Waiting;

        public virtual Problem Problem { get; set; } = null!;
        public virtual ApplicationUser HostUser { get; set; } = null!;
        public virtual ICollection<DuelParticipant> Participants { get; set; } = new List<DuelParticipant>();
        public virtual DuelMatch? Match { get; set; }
    }

    public enum DuelRoomStatus
    {
        Waiting,
        Ready,
        InProgress,
        Finished,
        Cancelled
    }
}
