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
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Contest Contest { get; set; } = new();

        public SelectList Tournaments { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync()
        {
            var tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
            Tournaments = new SelectList(tournaments, "Id", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ValidationErrors"] = string.Join("; ", errors);
                var tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
                Tournaments = new SelectList(tournaments, "Id", "Name");
                return Page();
            }

            var tournamentExists = await _context.Tournaments.AnyAsync(t => t.Id == Contest.TournamentId);
            if (!tournamentExists)
            {
                ModelState.AddModelError("Contest.TournamentId", "Selected tournament does not exist.");
                var tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
                Tournaments = new SelectList(tournaments, "Id", "Name");
                return Page();
            }

            if (Contest.EndTime <= Contest.StartTime)
            {
                ModelState.AddModelError("Contest.EndTime", "End time must be after start time.");
                var tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
                Tournaments = new SelectList(tournaments, "Id", "Name");
                return Page();
            }

            try
            {
                Contest.CreatedAt = DateTime.UtcNow;
                _context.Contests.Add(Contest);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not create contest: {ex.Message}";
                var tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
                Tournaments = new SelectList(tournaments, "Id", "Name");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
