using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Discussions
{
    [Authorize(Policy = "RequireAdminRole")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Discussion Discussion { get; set; } = new();

        public SelectList Problems { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discussion = await _context.Discussions.FindAsync(id);
            if (discussion == null)
            {
                return NotFound();
            }

            Discussion = discussion;
            await LoadProblemsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var existing = await _context.Discussions.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Validate ProblemId if not null
            if (Discussion.ProblemId.HasValue)
            {
                var problemExists = await _context.Problems.AnyAsync(p => p.Id == Discussion.ProblemId.Value);
                if (!problemExists)
                {
                    ModelState.AddModelError("Discussion.ProblemId", "Selected problem does not exist.");
                    await LoadProblemsAsync();
                    Discussion = existing;
                    return Page();
                }
            }

            if (await TryUpdateModelAsync(
                existing,
                "Discussion",
                d => d.Title, d => d.Content, d => d.ProblemId))
            {
                try
                {
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return RedirectToPage("./Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Discussions.AnyAsync(d => d.Id == existing.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Could not save discussion: {ex.Message}";
                }
            }

            await LoadProblemsAsync();
            Discussion = existing;
            return Page();
        }

        private async Task LoadProblemsAsync()
        {
            var problems = await _context.Problems
                .Where(p => p.IsPublic)
                .OrderBy(p => p.TitleVi)
                .Select(p => new { p.Id, p.TitleVi })
                .ToListAsync();
            Problems = new SelectList(problems, "Id", "TitleVi", Discussion.ProblemId);
        }
    }
}
