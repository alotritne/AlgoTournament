using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Announcements
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
        public Announcement Announcement { get; set; } = new();

        public SelectList Contests { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            Announcement = announcement;
            await LoadContestsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var existing = await _context.Announcements.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Validate ContestId if not null
            if (Announcement.ContestId.HasValue)
            {
                var contestExists = await _context.Contests.AnyAsync(c => c.Id == Announcement.ContestId.Value);
                if (!contestExists)
                {
                    ModelState.AddModelError("Announcement.ContestId", "Selected contest does not exist.");
                    await LoadContestsAsync();
                    Announcement = existing;
                    return Page();
                }
            }

            if (await TryUpdateModelAsync(
                existing,
                "Announcement",
                a => a.Title, a => a.Content, a => a.ContestId,
                a => a.ExpiresAt, a => a.IsActive, a => a.IsGlobal))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToPage("./Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Announcements.AnyAsync(a => a.Id == existing.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Could not save announcement: {ex.Message}";
                }
            }

            await LoadContestsAsync();
            Announcement = existing;
            return Page();
        }

        private async Task LoadContestsAsync()
        {
            var contests = await _context.Contests.OrderBy(c => c.Name).ToListAsync();
            Contests = new SelectList(contests, "Id", "Name", Announcement.ContestId);
        }
    }
}
