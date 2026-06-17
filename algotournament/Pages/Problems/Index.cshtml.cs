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
        }

        public int GetSolvedCount(int problemId)
        {
            // For now, return a random number. In a real implementation, this would count actual solved submissions
            return new Random().Next(100, 50000);
        }
    }
}