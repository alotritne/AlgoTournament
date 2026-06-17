using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Tournaments
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
        public Tournament Tournament { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Tournament = await _context.Tournaments
                .Include(t => t.Contests)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Tournament == null)
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

            var tournament = await _context.Tournaments
                .Include(t => t.Contests)
                    .ThenInclude(c => c.ContestProblems)
                .Include(t => t.Contests)
                    .ThenInclude(c => c.Participants)
                .Include(t => t.Contests)
                    .ThenInclude(c => c.Submissions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tournament == null)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                foreach (var contest in tournament.Contests.ToList())
                {
                    _context.Submissions.RemoveRange(contest.Submissions);
                    _context.ContestParticipants.RemoveRange(contest.Participants);
                    _context.ContestProblems.RemoveRange(contest.ContestProblems);
                    _context.Contests.Remove(contest);
                }

                _context.Tournaments.Remove(tournament);
                await _context.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not delete tournament: {ex.Message}";
                Tournament = tournament;
                return Page();
            }
        }
    }
}
