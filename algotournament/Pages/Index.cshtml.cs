using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalUsers { get; set; }
        public int TotalProblems { get; set; }
        public int TotalContests { get; set; }
        public int TotalSubmissions { get; set; }
        
        public List<Contest> UpcomingContests { get; set; } = new();
        public List<Announcement> Announcements { get; set; } = new();
        public List<ApplicationUser> TopUsers { get; set; } = new();

        public async Task OnGetAsync()
        {
            TotalUsers = await _context.Users.CountAsync();
            TotalProblems = await _context.Problems.CountAsync();
            TotalContests = await _context.Contests.CountAsync();
            TotalSubmissions = await _context.Submissions.CountAsync();

            // Get upcoming contests
            UpcomingContests = await _context.Contests
                .Where(c => c.StartTime > DateTime.UtcNow)
                .OrderBy(c => c.StartTime)
                .Take(5)
                .ToListAsync();

            // Get active announcements
            Announcements = await _context.Announcements
                .Where(a => a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow))
                .Where(a => a.IsGlobal)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Get top rated users
            TopUsers = await _context.Users
                .OrderByDescending(u => u.Rating)
                .Take(10)
                .ToListAsync();
        }
    }
}