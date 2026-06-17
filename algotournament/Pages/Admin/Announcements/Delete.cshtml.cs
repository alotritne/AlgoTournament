using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Announcements
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
        public Announcement Announcement { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Announcement = await _context.Announcements
                .Include(a => a.Creator)
                .Include(a => a.Contest)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Announcement == null)
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

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not delete announcement: {ex.Message}";
                Announcement = await _context.Announcements
                    .Include(a => a.Creator)
                    .Include(a => a.Contest)
                    .FirstOrDefaultAsync(m => m.Id == id) ?? new Announcement();
                return Page();
            }
        }
    }
}
