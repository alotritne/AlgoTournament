using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Submissions
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Submission> Submissions { get; set; } = new List<Submission>();

        public async Task OnGetAsync(int? problemId = null, int? contestId = null)
        {
            var query = _context.Submissions
                .Include(s => s.User)
                .Include(s => s.Problem)
                .Include(s => s.Contest)
                .AsQueryable();

            if (problemId.HasValue)
            {
                query = query.Where(s => s.ProblemId == problemId.Value);
            }

            if (contestId.HasValue)
            {
                query = query.Where(s => s.ContestId == contestId.Value);
            }

            Submissions = await query
                .OrderByDescending(s => s.SubmittedAt)
                .Take(500)
                .ToListAsync();
        }
    }
}
