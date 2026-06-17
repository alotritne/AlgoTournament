namespace algotournament.Models
{
    public class DuelParticipant
    {
        public int Id { get; set; }
        public int DuelRoomId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool IsReady { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public byte SlotIndex { get; set; }

        public virtual DuelRoom Room { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
