using System.Text.RegularExpressions;
using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Blog
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
        public BlogPost BlogPost { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            BlogPost.AuthorId = currentUser.Id;
            BlogPost.Slug = await GenerateUniqueSlugAsync(BlogPost.Title);
            BlogPost.PublishedAt = DateTime.UtcNow;

            _context.BlogPosts.Add(BlogPost);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task<string> GenerateUniqueSlugAsync(string title)
        {
            var baseSlug = GenerateSlug(title);
            if (string.IsNullOrEmpty(baseSlug))
            {
                baseSlug = "post";
            }

            var slug = baseSlug;
            var counter = 1;
            while (await _context.BlogPosts.AnyAsync(b => b.Slug == slug))
            {
                slug = $"{baseSlug}-{counter++}";
            }

            return slug;
        }

        private static string GenerateSlug(string title)
        {
            var slug = title.ToLowerInvariant().Trim();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            return slug.Trim('-');
        }
    }
}
