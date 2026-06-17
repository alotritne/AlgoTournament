using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Discussions
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

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadProblemsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ValidationErrors"] = string.Join("; ", errors);
                await LoadProblemsAsync();
                return Page();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (Discussion.ProblemId.HasValue)
            {
                var problemExists = await _context.Problems.AnyAsync(p => p.Id == Discussion.ProblemId.Value);
                if (!problemExists)
                {
                    ModelState.AddModelError("Discussion.ProblemId", "Selected problem does not exist.");
                    await LoadProblemsAsync();
                    return Page();
                }
            }

            try
            {
                Discussion.AuthorId = currentUser.Id;
                Discussion.CreatedAt = DateTime.UtcNow;
                Discussion.UpdatedAt = DateTime.UtcNow;
                Discussion.ReplyCount = 0;
                Discussion.ViewCount = 0;

                _context.Discussions.Add(Discussion);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not create discussion: {ex.Message}";
                await LoadProblemsAsync();
                return Page();
            }

            return RedirectToPage("./Index");
        }

        private async Task LoadProblemsAsync()
        {
            var problems = await _context.Problems
                .Where(p => p.IsPublic)
                .OrderBy(p => p.TitleVi)
                .Select(p => new { p.Id, p.TitleVi })
                .ToListAsync();
            Problems = new SelectList(problems, "Id", "TitleVi");
        }
    }
}
