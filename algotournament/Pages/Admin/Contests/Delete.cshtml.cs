using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Contests
{
    [Authorize(Policy = "RequireAdminRole")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Contest Contest { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Contest = await _context.Contests
                .Include(c => c.Tournament)
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Contest == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contest = await _context.Contests
                .Include(c => c.ContestProblems)
                .Include(c => c.Participants)
                .Include(c => c.Submissions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contest == null)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                _context.Submissions.RemoveRange(contest.Submissions);
                _context.ContestParticipants.RemoveRange(contest.Participants);
                _context.ContestProblems.RemoveRange(contest.ContestProblems);
                _context.Contests.Remove(contest);
                await _context.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not delete contest: {ex.Message}";
                Contest = contest;
                return Page();
            }
        }
    }
}
