using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.TestCases
{
    [Authorize(Policy = "RequireAdminRole")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TestCase TestCase { get; set; } = new();
        public Problem? Problem { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id, int problemId)
        {
            if (id == null)
            {
                return NotFound();
            }

            TestCase = await _context.TestCases.FindAsync(id);
            Problem = await _context.Problems.FindAsync(problemId);

            if (TestCase == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TestCase = await _context.TestCases.FindAsync(id);

            if (TestCase != null)
            {
                _context.TestCases.Remove(TestCase);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index", new { problemId = TestCase.ProblemId });
        }
    }
}
