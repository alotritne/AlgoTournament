using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace algotournament.Pages.Submissions
{
    [Authorize]
    public class SubmitModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubmitModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public Problem? Problem { get; set; }

        public class InputModel
        {
            [Required]
            public string SourceCode { get; set; } = string.Empty;

            [Required]
            public ProgrammingLanguage Language { get; set; } = ProgrammingLanguage.Cpp20;
        }

        public async Task<IActionResult> OnGetAsync(int problemId)
        {
            Problem = await _context.Problems.FindAsync(problemId);
            if (Problem == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int problemId)
        {
            if (!ModelState.IsValid)
            {
                Problem = await _context.Problems.FindAsync(problemId);
                return Page();
            }

            Problem = await _context.Problems.FindAsync(problemId);
            if (Problem == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Create submission
            var submission = new Submission
            {
                UserId = currentUser.Id,
                ProblemId = problemId,
                SourceCode = Input.SourceCode,
                Language = Input.Language,
                Status = SubmissionStatus.Queueing,
                SubmittedAt = DateTime.UtcNow
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            // Add to judge queue
            var judgeQueue = new JudgeQueue
            {
                SubmissionId = submission.Id,
                Status = JudgeQueueStatus.Pending,
                QueuedAt = DateTime.UtcNow,
                Priority = 0
            };

            _context.JudgeQueues.Add(judgeQueue);
            await _context.SaveChangesAsync();

            // Redirect to submission status
            return RedirectToPage("./Status", new { id = submission.Id });
        }
    }
}