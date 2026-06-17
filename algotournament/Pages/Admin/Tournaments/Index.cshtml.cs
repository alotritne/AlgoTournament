using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Tournaments
{
    [Authorize(Policy = "RequireAdminRole")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Tournament> Tournaments { get; set; } = new List<Tournament>();

        public async Task OnGetAsync()
        {
            Tournaments = await _context.Tournaments
                .Include(t => t.Contests)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
