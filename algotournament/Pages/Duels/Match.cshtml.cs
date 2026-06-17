using algotournament.Data;
using algotournament.Models;
using algotournament.Models.Dtos;
using algotournament.Services;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace algotournament.Pages.Duels
{
    [Authorize]
    public class MatchModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly DuelService _duelService;
        private readonly DuelMatchService _matchService;
        private readonly SubmissionService _submissionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MatchModel(
            ApplicationDbContext context,
            DuelService duelService,
            DuelMatchService matchService,
            SubmissionService submissionService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _duelService = duelService;
            _matchService = matchService;
            _submissionService = submissionService;
            _userManager = userManager;
        }

        public Problem? Problem { get; set; }
        public DuelMatchStateDto? MatchState { get; set; }
        public List<TestCase> SampleTestCases { get; set; } = new();
        public string? CurrentUserId { get; set; }

        [BindProperty]
        public SubmitInput Input { get; set; } = new();

        public class SubmitInput
        {
            [Required]
            public string SourceCode { get; set; } = string.Empty;
            public ProgrammingLanguage Language { get; set; } = ProgrammingLanguage.Cpp20;
        }

        public async Task<IActionResult> OnGetAsync(string roomCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            CurrentUserId = user.Id;
            var redirect = await LoadMatchAsync(roomCode, user.Id);
            if (redirect != null) return redirect;

            return Page();
        }

        public async Task<IActionResult> OnGetStateAsync(string roomCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var state = await _matchService.GetMatchStateAsync(roomCode, user.Id);
            if (state == null) return NotFound();

            return new JsonResult(state);
        }

        public async Task<IActionResult> OnPostSubmitAsync(string roomCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var state = await _matchService.GetMatchStateAsync(roomCode, user.Id);
            if (state == null) return NotFound();

            if (state.Status != DuelMatchStatus.InProgress)
            {
                return BadRequest(new { error = "Match is not in progress." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Invalid submission." });
            }

            try
            {
                await _matchService.ValidateSubmissionAsync(user.Id, state.MatchId);

                var submission = await _submissionService.CreateSubmissionAsync(
                    user.Id,
                    state.ProblemId,
                    Input.SourceCode,
                    Input.Language,
                    duelMatchId: state.MatchId);

                await _matchService.OnSubmissionCreatedAsync(submission.Id);

                return new JsonResult(new
                {
                    submissionId = submission.Id,
                    status = DuelMatchService.MapPublicStatus(submission.Status)
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        public string RenderMarkdown(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            return Markdown.ToHtml(markdown, pipeline);
        }

        private async Task<IActionResult?> LoadMatchAsync(string roomCode, string userId)
        {
            var lobby = await _duelService.GetLobbyStateAsync(roomCode, userId);
            if (lobby == null) return NotFound();

            if (lobby.Status != DuelRoomStatus.InProgress)
            {
                if (lobby.Status == DuelRoomStatus.Waiting || lobby.Status == DuelRoomStatus.Ready)
                {
                    return RedirectToPage("./Lobby", new { roomCode });
                }

                if (lobby.MatchId.HasValue)
                {
                    return RedirectToPage("./Result", new { id = lobby.MatchId.Value });
                }

                return RedirectToPage("./Join");
            }

            MatchState = await _matchService.GetMatchStateAsync(roomCode, userId);
            if (MatchState == null) return RedirectToPage("./Lobby", new { roomCode });

            if (MatchState.Status != DuelMatchStatus.InProgress)
            {
                return RedirectToPage("./Result", new { id = MatchState.MatchId });
            }

            Problem = await _context.Problems
                .Include(p => p.TestCases)
                .FirstOrDefaultAsync(p => p.Id == MatchState.ProblemId);

            if (Problem == null) return NotFound();

            SampleTestCases = Problem.TestCases
                .Where(tc => tc.IsSample)
                .OrderBy(tc => tc.Order)
                .ToList();

            return null;
        }
    }
}
