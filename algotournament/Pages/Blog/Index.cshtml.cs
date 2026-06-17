using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Blog
{
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
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.PublishedAt)
                .ToListAsync();
        }
    }
}
