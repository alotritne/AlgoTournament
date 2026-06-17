using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Blog
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public BlogPost? BlogPost { get; set; }

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            BlogPost = await _context.BlogPosts
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Slug == slug && b.IsPublished);

            if (BlogPost == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
