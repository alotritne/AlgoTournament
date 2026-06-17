using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Account
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ApplicationUser? CurrentUser { get; set; }
        public List<Submission> RecentSubmissions { get; set; } = new();
        public List<Contest> UpcomingContests { get; set; } = new();

        public async Task OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                CurrentUser = currentUser;

                // Get recent submissions
                RecentSubmissions = await _context.Submissions
                    .Include(s => s.Problem)
                    .Where(s => s.UserId == currentUser.Id)
                    .OrderByDescending(s => s.SubmittedAt)
                    .Take(10)
                    .ToListAsync();

                // Get upcoming contests
                UpcomingContests = await _context.Contests
                    .Where(c => c.StartTime > DateTime.UtcNow)
                    .OrderBy(c => c.StartTime)
                    .Take(5)
                    .ToListAsync();
            }
        }
    }
}