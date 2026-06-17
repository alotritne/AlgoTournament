using System.Text.RegularExpressions;
using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Blog
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
        public BlogPost BlogPost { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BlogPost = await _context.BlogPosts.FindAsync(id) ?? new BlogPost();
            if (BlogPost.Id == 0)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existing = await _context.BlogPosts.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == BlogPost.Id);
            if (existing == null)
            {
                return NotFound();
            }

            if (existing.Title != BlogPost.Title)
            {
                BlogPost.Slug = await GenerateUniqueSlugAsync(BlogPost.Title, BlogPost.Id);
            }
            else
            {
                BlogPost.Slug = existing.Slug;
            }

            BlogPost.UpdatedAt = DateTime.UtcNow;
            BlogPost.AuthorId = existing.AuthorId;
            BlogPost.PublishedAt = existing.PublishedAt;

            _context.Attach(BlogPost).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.BlogPosts.AnyAsync(b => b.Id == BlogPost.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToPage("./Index");
        }

        private async Task<string> GenerateUniqueSlugAsync(string title, int currentId)
        {
            var baseSlug = GenerateSlug(title);
            if (string.IsNullOrEmpty(baseSlug))
            {
                baseSlug = "post";
            }

            var slug = baseSlug;
            var counter = 1;
            while (await _context.BlogPosts.AnyAsync(b => b.Slug == slug && b.Id != currentId))
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
