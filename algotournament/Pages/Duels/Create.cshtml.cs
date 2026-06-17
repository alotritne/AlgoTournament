using algotournament.Data;
using algotournament.Models;
using algotournament.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace algotournament.Pages.Duels
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly DuelService _duelService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(ApplicationDbContext context, DuelService duelService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _duelService = duelService;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public SelectList Problems { get; set; } = null!;
        public List<int> DurationOptions { get; } = new() { 10, 15, 30, 60 };

        public class InputModel
        {
            [Required]
            [Display(Name = "Problem")]
            public int ProblemId { get; set; }

            [Required]
            [Display(Name = "Duration (minutes)")]
            public int DurationMinutes { get; set; } = 15;
        }

        public async Task OnGetAsync()
        {
            await LoadProblemsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadProblemsAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            try
            {
                var room = await _duelService.CreateRoomAsync(user.Id, Input.ProblemId, Input.DurationMinutes);
                return RedirectToPage("./Lobby", new { roomCode = room.RoomCode });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }

        private async Task LoadProblemsAsync()
        {
            var problems = await _context.Problems
                .Where(p => p.IsPublic)
                .OrderBy(p => p.TitleVi)
                .ToListAsync();
            Problems = new SelectList(problems, "Id", "TitleVi", Input.ProblemId);
        }
    }
}
