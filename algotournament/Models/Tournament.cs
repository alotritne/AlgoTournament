namespace algotournament.Models
{
    public class Tournament
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SeasonId { get; set; }
        public bool IsPrivate { get; set; } = false;
        public string? AccessCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual Season Season { get; set; } = null!;
        public virtual ICollection<Contest> Contests { get; set; } = new List<Contest>();
    }
}