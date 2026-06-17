using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Discussions
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
        public Discussion Discussion { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Discussion = await _context.Discussions
                .Include(d => d.Author)
                .Include(d => d.Problem)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (Discussion == null)
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

            var discussion = await _context.Discussions
                .Include(d => d.Replies)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (discussion == null)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                _context.DiscussionReplies.RemoveRange(discussion.Replies);
                _context.Discussions.Remove(discussion);
                await _context.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not delete discussion: {ex.Message}";
                Discussion = await _context.Discussions
                    .Include(d => d.Author)
                    .Include(d => d.Problem)
                    .FirstOrDefaultAsync(d => d.Id == id) ?? new Discussion();
                return Page();
            }
        }
    }
}
