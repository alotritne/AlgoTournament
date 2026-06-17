using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Account
{
    public class SubmissionsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubmissionsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ApplicationUser? CurrentUser { get; set; }
        public List<Submission> Submissions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            CurrentUser = currentUser;

            Submissions = await _context.Submissions
                .Include(s => s.Problem)
                .Where(s => s.UserId == currentUser.Id)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(500)
                .ToListAsync();

            return Page();
        }
    }
}
