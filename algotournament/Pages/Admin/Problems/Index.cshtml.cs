using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Problems
{
    [Authorize(Policy = "RequireAdminRole")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Problem> Problems { get; set; } = new List<Problem>();

        public async Task OnGetAsync()
        {
            Problems = await _context.Problems
                .Include(p => p.TestCases)
                .OrderBy(p => p.Id)
                .ToListAsync();
        }
    }
}
