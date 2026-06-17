using algotournament.Models;
using algotournament.Models.Dtos;
using algotournament.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace algotournament.Pages.Duels
{
    [Authorize]
    public class LobbyModel : PageModel
    {
        private readonly DuelService _duelService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LobbyModel(DuelService duelService, UserManager<ApplicationUser> userManager)
        {
            _duelService = duelService;
            _userManager = userManager;
        }

        public DuelLobbyStateDto? Lobby { get; set; }
        public string? CurrentUserId { get; set; }

        public async Task<IActionResult> OnGetAsync(string roomCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            CurrentUserId = user.Id;
            Lobby = await _duelService.GetLobbyStateAsync(roomCode, user.Id);

            if (Lobby == null) return NotFound();

            var isParticipant = Lobby.Participants.Any(p => p.UserId == user.Id);
            if (!isParticipant)
            {
                try
                {
                    await _duelService.JoinRoomAsync(user.Id, roomCode);
                    Lobby = await _duelService.GetLobbyStateAsync(roomCode, user.Id);
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = ex.Message;
                    return RedirectToPage("./Join");
                }
            }

            if (Lobby.Status == DuelRoomStatus.InProgress && Lobby.MatchId.HasValue)
            {
                return RedirectToPage("./Match", new { roomCode = Lobby.RoomCode });
            }

            if (Lobby.Status == DuelRoomStatus.Finished && Lobby.MatchId.HasValue)
            {
                return RedirectToPage("./Result", new { id = Lobby.MatchId.Value });
            }

            return Page();
        }
    }
}
