using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Blog
{
    [Authorize(Policy = "RequireAdminRole")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();

        public async Task OnGetAsync()
        {
            BlogPosts = await _context.BlogPosts
                .Include(b => b.Author)
                .OrderByDescending(b => b.PublishedAt)
                .ToListAsync();
        }
    }
}
