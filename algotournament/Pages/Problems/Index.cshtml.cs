using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Problems
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Problem> Problems { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Dictionary<int, int> SolvedCounts { get; set; } = new();

        public async Task OnGetAsync(int page = 1)
        {
            CurrentPage = page;

            var query = _context.Problems
                .Where(p => p.IsPublic)
                .OrderBy(p => p.Id);

            var totalProblems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalProblems / (double)PageSize);

            Problems = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Calculate solved counts for each problem
            var problemIds = Problems.Select(p => p.Id).ToList();
            var solvedCounts = await _context.Submissions
                .Where(s => problemIds.Contains(s.ProblemId) && s.Status == SubmissionStatus.Accepted)
                .GroupBy(s => s.ProblemId)
                .Select(g => new { ProblemId = g.Key, Count = g.Select(s => s.UserId).Distinct().Count() })
                .ToListAsync();

            SolvedCounts = solvedCounts.ToDictionary(x => x.ProblemId, x => x.Count);
        }

        public int GetSolvedCount(int problemId)
        {
            return SolvedCounts.TryGetValue(problemId, out var count) ? count : 0;
        }
    }
}