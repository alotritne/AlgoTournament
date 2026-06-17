using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Contests
{
    [Authorize(Policy = "RequireAdminRole")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Contest Contest { get; set; } = new();

        public SelectList Tournaments { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contest = await _context.Contests.FindAsync(id);
            if (contest == null)
            {
                return NotFound();
            }

            Contest = contest;
            var tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
            Tournaments = new SelectList(tournaments, "Id", "Name", Contest.TournamentId);
            // Ensure a placeholder option so the user can never post an invalid (0) TournamentId
            if (Contest.TournamentId == 0 || !tournaments.Any(t => t.Id == Contest.TournamentId))
            {
                Tournaments = new SelectList(tournaments, "Id", "Name", null);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var existing = await _context.Contests.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (Contest.EndTime <= Contest.StartTime)
            {
                ModelState.AddModelError("Contest.EndTime", "End time must be after start time.");
                var tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
                Tournaments = new SelectList(tournaments, "Id", "Name", Contest.TournamentId);
                return Page();
            }

            var tournamentExists = await _context.Tournaments.AnyAsync(t => t.Id == Contest.TournamentId);
            if (!tournamentExists)
            {
                ModelState.AddModelError("Contest.TournamentId", "Selected tournament does not exist.");
                var tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
                Tournaments = new SelectList(tournaments, "Id", "Name", Contest.TournamentId);
                return Page();
            }

            if (await TryUpdateModelAsync(
                existing,
                "Contest",
                c => c.Name, c => c.Description, c => c.TournamentId,
                c => c.StartTime, c => c.EndTime, c => c.DurationMinutes,
                c => c.ScoringMode, c => c.IsRated))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToPage("./Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Contests.AnyAsync(e => e.Id == existing.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Could not save contest: {ex.Message}";
                }
            }

            var tournaments2 = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
            Tournaments = new SelectList(tournaments2, "Id", "Name", Contest.TournamentId);
            return Page();
        }
    }
}
