using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Discussions
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
        public Discussion Discussion { get; set; } = new();

        public SelectList Problems { get; set; } = null!;

        public async Task OnGetAsync()
        {
            var problems = await _context.Problems
                .Where(p => p.IsPublic)
                .Select(p => new { p.Id, p.TitleVi })
                .ToListAsync();

            Problems = new SelectList(problems, "Id", "TitleVi");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Discussion.AuthorId = currentUser.Id;
            Discussion.CreatedAt = DateTime.UtcNow;
            Discussion.UpdatedAt = DateTime.UtcNow;
            Discussion.ReplyCount = 0;
            Discussion.ViewCount = 0;

            _context.Discussions.Add(Discussion);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
