using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace algotournament.Pages.Account
{
    public class BannedModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BannedModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public string? BanReason { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId != null)
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.IsBanned, u.BanReason })
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    BanReason = user.BanReason;
                    
                    // If user is not banned, redirect to home
                    if (!user.IsBanned)
                    {
                        return RedirectToPage("/Index");
                    }
                }
            }

            return Page();
        }
    }
}
