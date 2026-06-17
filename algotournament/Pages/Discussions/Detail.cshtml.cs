using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Discussions
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Discussion? Discussion { get; set; }
        public IList<DiscussionReply> Replies { get; set; } = new List<DiscussionReply>();

        [BindProperty]
        public string NewReplyContent { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Discussion = await _context.Discussions
                .Include(d => d.Author)
                .Include(d => d.Problem)
                .Include(d => d.Replies)
                    .ThenInclude(r => r.Author)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (Discussion == null)
            {
                return NotFound();
            }

            Replies = Discussion.Replies.OrderBy(r => r.CreatedAt).ToList();

            // Increment view count
            Discussion.ViewCount++;
            await _context.SaveChangesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Discussion = await _context.Discussions
                .Include(d => d.Author)
                .Include(d => d.Problem)
                .Include(d => d.Replies)
                    .ThenInclude(r => r.Author)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (Discussion == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var reply = new DiscussionReply
            {
                DiscussionId = id,
                AuthorId = currentUser.Id,
                Content = NewReplyContent,
                CreatedAt = DateTime.UtcNow
            };

            _context.DiscussionReplies.Add(reply);
            Discussion.ReplyCount++;
            Discussion.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToPage(new { id });
        }
    }
}
