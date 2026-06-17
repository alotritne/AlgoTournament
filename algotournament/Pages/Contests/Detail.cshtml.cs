using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Contests
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Contest? Contest { get; set; }
        public List<ContestProblem> ContestProblems { get; set; } = new();
        public bool IsRegistered { get; set; }
        public bool CanRegister { get; set; }
        public bool HasStarted { get; set; }
        public bool HasEnded { get; set; }
        public int ParticipantCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Contest = await _context.Contests
                .Include(c => c.ContestProblems)
                .ThenInclude(cp => cp.Problem)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (Contest == null)
            {
                return NotFound();
            }

            ContestProblems = Contest.ContestProblems.ToList();

            var now = DateTime.UtcNow;
            HasStarted = now >= Contest.StartTime;
            HasEnded = now > Contest.EndTime;
            CanRegister = !HasEnded && !HasStarted;

            // Check if user is registered
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                IsRegistered = await _context.ContestParticipants
                    .AnyAsync(cp => cp.ContestId == id && cp.UserId == currentUser.Id);
            }

            // Get participant count
            ParticipantCount = await _context.ContestParticipants
                .CountAsync(cp => cp.ContestId == id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var contest = await _context.Contests.FindAsync(id);
            if (contest == null)
            {
                return NotFound();
            }

            var now = DateTime.UtcNow;
            if (now >= contest.StartTime || now > contest.EndTime)
            {
                ModelState.AddModelError(string.Empty, "Cannot register for this contest.");
                return await OnGetAsync(id);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Check if already registered
            var existing = await _context.ContestParticipants
                .AnyAsync(cp => cp.ContestId == id && cp.UserId == currentUser.Id);

            if (existing)
            {
                ModelState.AddModelError(string.Empty, "You are already registered for this contest.");
                return await OnGetAsync(id);
            }

            // Register user
            var participant = new ContestParticipant
            {
                ContestId = id,
                UserId = currentUser.Id,
                RegisteredAt = DateTime.UtcNow
            };

            _context.ContestParticipants.Add(participant);
            await _context.SaveChangesAsync();

            IsRegistered = true;
            return await OnGetAsync(id);
        }

        public string GetTimeRemaining()
        {
            if (Contest == null) return "N/A";

            var now = DateTime.UtcNow;
            if (now < Contest.StartTime)
            {
                var timeUntilStart = Contest.StartTime - now;
                return $"{(int)timeUntilStart.TotalHours:D2}:{timeUntilStart.Minutes:D2}:{timeUntilStart.Seconds:D2}";
            }
            else if (now <= Contest.EndTime)
            {
                var timeUntilEnd = Contest.EndTime - now;
                return $"{(int)timeUntilEnd.TotalHours:D2}:{timeUntilEnd.Minutes:D2}:{timeUntilEnd.Seconds:D2}";
            }
            else
            {
                return "Contest Ended";
            }
        }

        public string GetContestStatus()
        {
            if (Contest == null) return "Unknown";

            var now = DateTime.UtcNow;
            if (now < Contest.StartTime)
            {
                return "Before contest begins...";
            }
            else if (now <= Contest.EndTime)
            {
                return "Contest is running!";
            }
            else
            {
                return "Contest has ended.";
            }
        }

        public int GetProblemSolvedCount(int problemId)
        {
            // For now, return a random number. In a real implementation, this would count actual solved submissions
            return new Random().Next(100, 10000);
        }
    }
}