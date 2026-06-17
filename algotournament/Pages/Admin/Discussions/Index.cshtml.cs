using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace algotournament.Pages.Admin.Discussions
{
    [Authorize(Policy = "RequireAdminRole")]
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
                .Include(d => d.Problem)
                .OrderByDescending(d => d.UpdatedAt)
                .ToListAsync();
        }
    }
}
