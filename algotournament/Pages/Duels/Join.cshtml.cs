using algotournament.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace algotournament.Pages.Duels
{
    [Authorize]
    public class JoinModel : PageModel
    {
        private readonly DuelService _duelService;
        private readonly UserManager<Models.ApplicationUser> _userManager;

        public JoinModel(DuelService duelService, UserManager<Models.ApplicationUser> userManager)
        {
            _duelService = duelService;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [StringLength(6, MinimumLength = 6)]
            [Display(Name = "Room Code")]
            public string RoomCode { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            try
            {
                var room = await _duelService.JoinRoomAsync(user.Id, Input.RoomCode);
                return RedirectToPage("./Lobby", new { roomCode = room.RoomCode });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}
