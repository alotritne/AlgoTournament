using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Announcements
{
    [Authorize(Policy = "RequireAdminRole")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Announcement Announcement { get; set; } = new();

        public SelectList Contests { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadContestsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ValidationErrors"] = string.Join("; ", errors);
                await LoadContestsAsync();
                return Page();
            }

            if (Announcement.ContestId.HasValue)
            {
                var contestExists = await _context.Contests.AnyAsync(c => c.Id == Announcement.ContestId.Value);
                if (!contestExists)
                {
                    ModelState.AddModelError("Announcement.ContestId", "Selected contest does not exist.");
                    await LoadContestsAsync();
                    return Page();
                }
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                Announcement.CreatedBy = currentUser?.Id ?? string.Empty;
                Announcement.CreatedAt = DateTime.UtcNow;

                _context.Announcements.Add(Announcement);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not create announcement: {ex.Message}";
                await LoadContestsAsync();
                return Page();
            }

            return RedirectToPage("./Index");
        }

        private async Task LoadContestsAsync()
        {
            var contests = await _context.Contests.OrderBy(c => c.Name).ToListAsync();
            Contests = new SelectList(contests, "Id", "Name");
        }
    }
}
