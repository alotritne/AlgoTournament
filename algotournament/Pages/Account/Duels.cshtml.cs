using algotournament.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Account
{
    [Authorize]
    public class DuelsModel : PageModel
    {
        private readonly algotournament.Data.ApplicationDbContext _context;
        private readonly UserManager<Models.ApplicationUser> _userManager;

        public DuelsModel(algotournament.Data.ApplicationDbContext context, UserManager<Models.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<DuelHistoryItemDto> History { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var matches = await _context.DuelMatches
                .Include(m => m.Room)
                    .ThenInclude(r => r.Problem)
                .Where(m => m.Player1UserId == user.Id || m.Player2UserId == user.Id)
                .Where(m => m.Status == Models.DuelMatchStatus.Finished ||
                            m.Status == Models.DuelMatchStatus.ForceEnded)
                .OrderByDescending(m => m.EndedAt ?? m.StartedAt)
                .Take(50)
                .ToListAsync();

            var opponentIds = matches
                .Select(m => m.Player1UserId == user.Id ? m.Player2UserId : m.Player1UserId)
                .Distinct()
                .ToList();

            var opponents = await _context.Users
                .Where(u => opponentIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Handle);

            History = matches.Select(m =>
            {
                var isPlayer1 = m.Player1UserId == user.Id;
                var myScore = isPlayer1 ? m.FinalScorePlayer1 : m.FinalScorePlayer2;
                var oppScore = isPlayer1 ? m.FinalScorePlayer2 : m.FinalScorePlayer1;
                var oppId = isPlayer1 ? m.Player2UserId : m.Player1UserId;
                string result;
                if (m.WinnerUserId == null) result = "Draw";
                else if (m.WinnerUserId == user.Id) result = "Win";
                else result = "Loss";

                return new DuelHistoryItemDto
                {
                    MatchId = m.Id,
                    Date = m.EndedAt ?? m.StartedAt,
                    OpponentHandle = opponents.GetValueOrDefault(oppId, "unknown"),
                    ProblemTitle = m.Room.Problem.TitleVi,
                    Result = result,
                    MyScore = myScore,
                    OpponentScore = oppScore
                };
            }).ToList();
        }
    }
}
