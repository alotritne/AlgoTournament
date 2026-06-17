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

        public async Task OnGetAsync()
        {
            var contests = await _context.Contests.OrderBy(c => c.Name).ToListAsync();
            Contests = new SelectList(contests, "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var contests = await _context.Contests.OrderBy(c => c.Name).ToListAsync();
                Contests = new SelectList(contests, "Id", "Name");
                return Page();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            Announcement.CreatedBy = currentUser?.Id ?? string.Empty;
            Announcement.CreatedAt = DateTime.UtcNow;

            _context.Announcements.Add(Announcement);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
