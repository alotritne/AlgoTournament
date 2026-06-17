using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Submissions
{
    [Authorize]
    public class StatusModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public StatusModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Submission? Submission { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Submission = await _context.Submissions
                .Include(s => s.User)
                .Include(s => s.Problem)
                .Include(s => s.Contest)
                .Include(s => s.Results)
                    .ThenInclude(r => r.TestCase)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (Submission == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
