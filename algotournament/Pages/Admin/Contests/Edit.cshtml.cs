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

            Contest = await _context.Contests.FindAsync(id);

            if (Contest == null)
            {
                return NotFound();
            }

            var tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
            Tournaments = new SelectList(tournaments, "Id", "Name", Contest.TournamentId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
                Tournaments = new SelectList(tournaments, "Id", "Name", Contest.TournamentId);
                return Page();
            }

            _context.Attach(Contest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Contests.Any(e => e.Id == Contest.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }
    }
}
