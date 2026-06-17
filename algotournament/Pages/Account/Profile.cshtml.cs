using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ApplicationUser? ProfileUser { get; set; }
        public List<Problem> SolvedProblems { get; set; } = new();
        public Dictionary<int, DateTime> SolvedDates { get; set; } = new();
        public List<Submission> RecentSubmissions { get; set; } = new();
        public bool IsCurrentUser { get; set; }
        public int Rank { get; set; }

        public async Task<IActionResult> OnGetAsync(string? handle = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ApplicationUser? profileUser;

            if (string.IsNullOrEmpty(handle))
            {
                if (currentUser == null)
                {
                    return RedirectToPage("/Account/Login");
                }
                profileUser = currentUser;
                IsCurrentUser = true;
            }
            else
            {
                profileUser = await _context.Users.FirstOrDefaultAsync(u => u.Handle == handle);
                if (profileUser == null)
                {
                    return NotFound();
                }
                IsCurrentUser = currentUser != null && currentUser.Id == profileUser.Id;
            }

            ProfileUser = profileUser;

            // Calculate rank
            Rank = await _context.Users.CountAsync(u => u.Rating > ProfileUser.Rating) + 1;

            // Get solved problems
            var acceptedSubmissions = await _context.Submissions
                .Include(s => s.Problem)
                .Where(s => s.UserId == ProfileUser.Id && s.Status == SubmissionStatus.Accepted)
                .OrderBy(s => s.SubmittedAt)
                .ToListAsync();

            // Get unique solved problems
            var uniqueSolved = acceptedSubmissions
                .GroupBy(s => s.ProblemId)
                .Select(g => g.First())
                .ToList();

            SolvedProblems = uniqueSolved.Select(s => s.Problem!).Where(p => p != null).ToList();
            SolvedDates = uniqueSolved.ToDictionary(s => s.ProblemId, s => s.SubmittedAt);

            // Get recent submissions
            RecentSubmissions = await _context.Submissions
                .Include(s => s.Problem)
                .Where(s => s.UserId == ProfileUser.Id)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(20)
                .ToListAsync();

            return Page();
        }
    }
}