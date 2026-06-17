using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Seasons
{
    [Authorize(Policy = "RequireAdminRole")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Season> Seasons { get; set; } = new List<Season>();

        public async Task OnGetAsync()
        {
            Seasons = await _context.Seasons
                .Include(s => s.Tournaments)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
}
