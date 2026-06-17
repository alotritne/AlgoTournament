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

        public async Task OnGetAsync()
        {
            var tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
            Tournaments = new SelectList(tournaments, "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
                Tournaments = new SelectList(tournaments, "Id", "Name");
                return Page();
            }

            Contest.CreatedAt = DateTime.UtcNow;
            _context.Contests.Add(Contest);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
