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

            Discussion = await _context.Discussions.FindAsync(id) ?? new Discussion();
            if (Discussion.Id == 0)
            {
                return NotFound();
            }

            var problems = await _context.Problems
                .Where(p => p.IsPublic)
                .Select(p => new { p.Id, p.TitleVi })
                .ToListAsync();

            Problems = new SelectList(problems, "Id", "Title", Discussion.ProblemId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var problems = await _context.Problems
                    .Where(p => p.IsPublic)
                    .Select(p => new { p.Id, p.TitleVi })
                    .ToListAsync();

                Problems = new SelectList(problems, "Id", "Title", Discussion.ProblemId);
                return Page();
            }

            var existing = await _context.Discussions.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == Discussion.Id);
            if (existing == null)
            {
                return NotFound();
            }

            Discussion.AuthorId = existing.AuthorId;
            Discussion.CreatedAt = existing.CreatedAt;
            Discussion.ReplyCount = existing.ReplyCount;
            Discussion.ViewCount = existing.ViewCount;
            Discussion.UpdatedAt = DateTime.UtcNow;

            _context.Attach(Discussion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Discussions.AnyAsync(d => d.Id == Discussion.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToPage("./Index");
        }
    }
}
