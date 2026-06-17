using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.TestCases
{
    [Authorize(Policy = "RequireAdminRole")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TestCase TestCase { get; set; } = new();
        public Problem? Problem { get; set; }

        public async Task OnGetAsync(int problemId)
        {
            TestCase.ProblemId = problemId;
            Problem = await _context.Problems.FindAsync(problemId);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Problem = await _context.Problems.FindAsync(TestCase.ProblemId);
                return Page();
            }

            // Validate ProblemId exists
            var problemExists = await _context.Problems.AnyAsync(p => p.Id == TestCase.ProblemId);
            if (!problemExists)
            {
                ModelState.AddModelError("TestCase.ProblemId", "Problem does not exist.");
                Problem = await _context.Problems.FindAsync(TestCase.ProblemId);
                return Page();
            }

            TestCase.CreatedAt = DateTime.UtcNow;
            TestCase.Problem = null; // Prevent navigation property validation issue
            _context.TestCases.Add(TestCase);

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index", new { problemId = TestCase.ProblemId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Could not save test case: {ex.Message}");
                Problem = await _context.Problems.FindAsync(TestCase.ProblemId);
                return Page();
            }
        }
    }
}
