using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Contests
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Contest> UpcomingContests { get; set; } = new();
        public List<Contest> PastContests { get; set; } = new();
        public List<Announcement> ContestAnnouncements { get; set; } = new();

        public async Task OnGetAsync()
        {
            var now = DateTime.UtcNow;

            // Get upcoming contests
            UpcomingContests = await _context.Contests
                .Include(c => c.ContestProblems)
                .ThenInclude(cp => cp.Problem)
                .Where(c => c.StartTime > now)
                .OrderBy(c => c.StartTime)
                .Take(20)
                .ToListAsync();

            // Get past contests
            PastContests = await _context.Contests
                .Where(c => c.EndTime < now)
                .OrderByDescending(c => c.StartTime)
                .Take(20)
                .ToListAsync();

            // Get contest announcements
            ContestAnnouncements = await _context.Announcements
                .Where(a => a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > now))
                .Where(a => !a.IsGlobal && a.ContestId != null)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();
        }

        public List<string> GetContestWriters(Contest contest)
        {
            // For now, return a placeholder. In a real implementation, this would be based on contest writers
            return new List<string> { "admin" };
        }
    }
}