using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Seasons
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
        public Season Season { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Season = await _context.Seasons
                .Include(s => s.Tournaments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Season == null)
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

            var season = await _context.Seasons
                .Include(s => s.Tournaments)
                    .ThenInclude(t => t.Contests)
                        .ThenInclude(c => c.ContestProblems)
                .Include(s => s.Tournaments)
                    .ThenInclude(t => t.Contests)
                        .ThenInclude(c => c.Participants)
                .Include(s => s.Tournaments)
                    .ThenInclude(t => t.Contests)
                        .ThenInclude(c => c.Submissions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (season == null)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                // Delete all related data in proper order to respect FK constraints
                foreach (var tournament in season.Tournaments.ToList())
                {
                    foreach (var contest in tournament.Contests.ToList())
                    {
                        _context.Submissions.RemoveRange(contest.Submissions);
                        _context.ContestParticipants.RemoveRange(contest.Participants);
                        _context.ContestProblems.RemoveRange(contest.ContestProblems);
                        _context.Contests.Remove(contest);
                    }
                    _context.Tournaments.Remove(tournament);
                }

                _context.Seasons.Remove(season);
                await _context.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not delete season: {ex.Message}";
                Season = season;
                return Page();
            }
        }
    }
}
