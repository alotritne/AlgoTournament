using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace algotournament.Pages.Admin
{
    [Authorize(Policy = "RequireAdminRole")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalUsers { get; set; }
        public int ActiveContests { get; set; }
        public int TotalProblems { get; set; }
        public int RecentSubmissions { get; set; }
        public int PendingJudges { get; set; }
        public string SystemUptime { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            TotalUsers = await _context.Users.CountAsync();
            
            var now = DateTime.UtcNow;
            ActiveContests = await _context.Contests
                .Where(c => c.StartTime <= now && c.EndTime >= now)
                .CountAsync();
            
            TotalProblems = await _context.Problems.CountAsync();
            
            var yesterday = now.AddDays(-1);
            RecentSubmissions = await _context.Submissions
                .Where(s => s.SubmittedAt >= yesterday)
                .CountAsync();
            
            PendingJudges = await _context.JudgeQueues
                .Where(jq => jq.Status == JudgeQueueStatus.Pending)
                .CountAsync();
            
            // Calculate system uptime (simplified)
            var uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;
            SystemUptime = uptime.ToString(@"dd\:hh\:mm\:ss");
        }

        public async Task<IActionResult> OnPostClearCacheAsync()
        {
            // Add cache clearing logic here
            TempData["Message"] = "Cache cleared successfully";
            return RedirectToPage();
        }
    }
}