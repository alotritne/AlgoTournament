using algotournament.Data;
using algotournament.Models;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Problems
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Problem? Problem { get; set; }
        public List<TestCase> SampleTestCases { get; set; } = new();
        public string CurrentLanguage { get; set; } = "vi";
        public bool ShowTranslationBadge { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(int id, string? lang = null)
        {
            Problem = await _context.Problems
                .Include(p => p.TestCases)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (Problem == null)
            {
                return NotFound();
            }

            // Get sample test cases only
            SampleTestCases = Problem.TestCases
                .Where(tc => tc.IsSample)
                .OrderBy(tc => tc.Order)
                .ToList();

            // Determine current language
            CurrentLanguage = lang ?? Request.Cookies["preferredLanguage"] ?? "vi";
            
            // Check if we should show translation badge (when viewing English but translation is missing)
            if (CurrentLanguage == "en" && !Problem.HasEnglishTranslation())
            {
                ShowTranslationBadge = true;
            }

            return Page();
        }

        public string RenderMarkdown(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return string.Empty;

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            return Markdig.Markdown.ToHtml(markdown, pipeline);
        }

        public string GetTitle()
        {
            return Problem?.GetTitle(CurrentLanguage) ?? "";
        }

        public string GetStatement()
        {
            return Problem?.GetStatement(CurrentLanguage) ?? "";
        }

        public string GetInputDescription()
        {
            return Problem?.GetInputDescription(CurrentLanguage) ?? "";
        }

        public string GetOutputDescription()
        {
            return Problem?.GetOutputDescription(CurrentLanguage) ?? "";
        }

        public string GetConstraints()
        {
            return Problem?.GetConstraints(CurrentLanguage) ?? "";
        }

        public string GetExplanation()
        {
            return Problem?.GetExplanation(CurrentLanguage) ?? "";
        }
    }
}