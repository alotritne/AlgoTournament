using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.TestCases
{
    [Authorize(Policy = "RequireAdminRole")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int ProblemId { get; set; }
        public Problem? Problem { get; set; }
        public IList<TestCase> TestCases { get; set; } = new List<TestCase>();

        public async Task OnGetAsync(int problemId)
        {
            ProblemId = problemId;
            Problem = await _context.Problems.FindAsync(problemId);
            
            if (Problem != null)
            {
                TestCases = await _context.TestCases
                    .Where(tc => tc.ProblemId == problemId)
                    .OrderBy(tc => tc.Order)
                    .ToListAsync();
            }
        }
    }
}
