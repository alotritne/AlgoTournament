using algotournament.Models.Dtos;
using algotournament.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Duels
{
    [Authorize]
    public class ResultModel : PageModel
    {
        private readonly algotournament.Data.ApplicationDbContext _context;
        private readonly DuelScoreService _scoreService;
        private readonly UserManager<Models.ApplicationUser> _userManager;

        public ResultModel(
            algotournament.Data.ApplicationDbContext context,
            DuelScoreService scoreService,
            UserManager<Models.ApplicationUser> userManager)
        {
            _context = context;
            _scoreService = scoreService;
            _userManager = userManager;
        }

        public DuelResultDto? Result { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var match = await _context.DuelMatches.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (match == null) return NotFound();

            if (match.Player1UserId != user.Id && match.Player2UserId != user.Id &&
                !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            Result = await _scoreService.BuildResultDtoAsync(id, user.Id);
            return Page();
        }
    }
}
