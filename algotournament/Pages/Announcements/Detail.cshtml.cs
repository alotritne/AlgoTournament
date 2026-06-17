using algotournament.Data;
using algotournament.Models;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Announcements
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Announcement? Announcement { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var now = DateTime.UtcNow;

            Announcement = await _context.Announcements
                .Include(a => a.Creator)
                .Include(a => a.Contest)
                .FirstOrDefaultAsync(a => a.Id == id
                    && a.IsActive
                    && (a.ExpiresAt == null || a.ExpiresAt > now));

            if (Announcement == null)
            {
                return NotFound();
            }

            return Page();
        }

        public string RenderMarkdown(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            return Markdown.ToHtml(markdown, pipeline);
        }
    }
}
