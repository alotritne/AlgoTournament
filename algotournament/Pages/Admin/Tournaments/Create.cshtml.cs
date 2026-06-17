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
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Tournament Tournament { get; set; } = new();

        public SelectList SeasonSelectList { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync()
        {
            var seasons = await _context.Seasons.OrderBy(s => s.Name).ToListAsync();
            SeasonSelectList = new SelectList(seasons, "Id", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Clear the navigation collection so it doesn't get re-added on save
            Tournament.Contests = new List<Contest>();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ValidationErrors"] = string.Join("; ", errors);
                var seasons = await _context.Seasons.OrderBy(s => s.Name).ToListAsync();
                SeasonSelectList = new SelectList(seasons, "Id", "Name");
                return Page();
            }

            var seasonExists = await _context.Seasons.AnyAsync(s => s.Id == Tournament.SeasonId);
            if (!seasonExists)
            {
                ModelState.AddModelError("Tournament.SeasonId", "Selected season does not exist.");
                var seasons = await _context.Seasons.OrderBy(s => s.Name).ToListAsync();
                SeasonSelectList = new SelectList(seasons, "Id", "Name");
                return Page();
            }

            try
            {
                Tournament.CreatedAt = DateTime.UtcNow;
                _context.Tournaments.Add(Tournament);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not create tournament: {ex.Message}";
                var seasons = await _context.Seasons.OrderBy(s => s.Name).ToListAsync();
                SeasonSelectList = new SelectList(seasons, "Id", "Name");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
