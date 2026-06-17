using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Discussions
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Discussion> Discussions { get; set; } = new List<Discussion>();

        public async Task OnGetAsync()
        {
            Discussions = await _context.Discussions
                .Include(d => d.Author)
                .OrderByDescending(d => d.UpdatedAt)
                .ToListAsync();
        }
    }
}
