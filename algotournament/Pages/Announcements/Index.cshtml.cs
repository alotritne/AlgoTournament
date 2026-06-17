using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Announcements
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Announcement> Announcements { get; set; } = new List<Announcement>();

        public async Task OnGetAsync()
        {
            var now = DateTime.UtcNow;

            Announcements = await _context.Announcements
                .Include(a => a.Creator)
                .Include(a => a.Contest)
                .Where(a => a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > now))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}
