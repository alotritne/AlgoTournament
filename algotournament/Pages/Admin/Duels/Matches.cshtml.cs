using algotournament.Data;
using algotournament.Models;
using algotournament.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Duels
{
    [Authorize(Policy = "RequireAdminRole")]
    public class MatchesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly DuelMatchService _matchService;
        private readonly DuelScoreService _scoreService;

        public MatchesModel(ApplicationDbContext context, DuelMatchService matchService, DuelScoreService scoreService)
        {
            _context = context;
            _matchService = matchService;
            _scoreService = scoreService;
        }

        public List<DuelMatch> Matches { get; set; } = new();

        public async Task OnGetAsync()
        {
            Matches = await _context.DuelMatches
                .Include(m => m.Room)
                    .ThenInclude(r => r.Problem)
                .OrderByDescending(m => m.StartedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostForceEndAsync(int matchId)
        {
            await _matchService.ForceEndMatchAsync(matchId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteMatchAsync(int matchId)
        {
            await _matchService.DeleteMatchAsync(matchId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRecalculateAsync(int matchId)
        {
            await _scoreService.RecalculateMatchResultAsync(matchId);
            return RedirectToPage();
        }
    }
}
