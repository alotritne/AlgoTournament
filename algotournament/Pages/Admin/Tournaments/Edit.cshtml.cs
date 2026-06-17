using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Tournaments
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
        public Tournament Tournament { get; set; } = new();

        public SelectList SeasonSelectList { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }

            Tournament = tournament;
            var seasons = await _context.Seasons.OrderBy(s => s.Name).ToListAsync();
            SeasonSelectList = new SelectList(seasons, "Id", "Name", Tournament.SeasonId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var existing = await _context.Tournaments.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Validate SeasonId exists
            var seasonExists = await _context.Seasons.AnyAsync(s => s.Id == Tournament.SeasonId);
            if (!seasonExists)
            {
                ModelState.AddModelError("Tournament.SeasonId", "Selected season does not exist.");
                var seasons = await _context.Seasons.OrderBy(s => s.Name).ToListAsync();
                SeasonSelectList = new SelectList(seasons, "Id", "Name", Tournament.SeasonId);
                Tournament = existing;
                return Page();
            }

            if (await TryUpdateModelAsync(
                existing,
                "Tournament",
                t => t.Name, t => t.Description, t => t.SeasonId,
                t => t.IsPrivate, t => t.AccessCode, t => t.IsActive))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToPage("./Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Tournaments.AnyAsync(e => e.Id == existing.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Could not save tournament: {ex.Message}";
                }
            }

            var seasonsList = await _context.Seasons.OrderBy(s => s.Name).ToListAsync();
            SeasonSelectList = new SelectList(seasonsList, "Id", "Name", Tournament.SeasonId);
            Tournament = existing;
            return Page();
        }
    }
}
