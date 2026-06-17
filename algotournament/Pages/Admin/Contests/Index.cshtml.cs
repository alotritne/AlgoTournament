using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace algotournament.Pages.Admin.Contests
{
    [Authorize(Policy = "RequireAdminRole")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Contest> Contests { get; set; } = new List<Contest>();

        public async Task OnGetAsync()
        {
            Contests = await _context.Contests
                .Include(c => c.Tournament)
                .Include(c => c.Participants)
                .OrderByDescending(c => c.StartTime)
                .ToListAsync();
        }
    }
}
