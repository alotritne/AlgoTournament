using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Contests
{
    [Authorize]
    public class RankingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RankingsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Contest? Contest { get; set; }
        public IList<Ranking> Rankings { get; set; } = new List<Ranking>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Contest = await _context.Contests.FindAsync(id);

            if (Contest == null)
            {
                return NotFound();
            }

            Rankings = await _context.Rankings
                .Include(r => r.User)
                .Where(r => r.ContestId == id)
                .OrderBy(r => r.Rank)
                .ToListAsync();

            return Page();
        }
    }
}
