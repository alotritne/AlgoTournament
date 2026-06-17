using System.ComponentModel.DataAnnotations;
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
            var logger = HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("BlogCreate");

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kv => kv.Value!.Errors.Count > 0)
                    .Select(kv => $"{kv.Key}: {string.Join(", ", kv.Value!.Errors.Select(e => e.ErrorMessage))}");
                logger.LogWarning("BlogCreate ModelState invalid: {Errors}", string.Join(" | ", errors));
                return Page();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                logger.LogWarning("BlogCreate currentUser is null");
                return RedirectToPage("/Account/Login");
            }

            BlogPost.AuthorId = currentUser.Id;
            BlogPost.Slug = await GenerateUniqueSlugAsync(BlogPost.Title);
            BlogPost.PublishedAt = DateTime.UtcNow;
            BlogPost.Author = null!;

            try
            {
                _context.BlogPosts.Add(BlogPost);
                await _context.SaveChangesAsync();
                logger.LogInformation("BlogPost created id={Id} slug={Slug}", BlogPost.Id, BlogPost.Slug);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save BlogPost");
                ModelState.AddModelError(string.Empty, $"Could not save: {ex.Message}");
                return Page();
            }

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
