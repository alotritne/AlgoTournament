using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin
{
    [Authorize(Policy = "RequireAdminRole")]
    public class RejudgeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RejudgeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string? Message { get; set; }

        public async Task<IActionResult> OnPostRejudgeSubmissionAsync(int submissionId)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission == null)
            {
                Message = "Submission not found";
                return Page();
            }

            // Reset submission status
            submission.Status = SubmissionStatus.Queueing;
            submission.Score = 0;
            submission.ExecutionTimeMs = 0;
            submission.CompileError = null;
            submission.RuntimeError = null;
            submission.JudgedAt = null;

            // Clear existing results
            var existingResults = await _context.SubmissionResults
                .Where(sr => sr.SubmissionId == submissionId)
                .ToListAsync();
            _context.SubmissionResults.RemoveRange(existingResults);

            // Add to judge queue
            var existingQueue = await _context.JudgeQueues
                .FirstOrDefaultAsync(jq => jq.SubmissionId == submissionId);
            
            if (existingQueue != null)
            {
                existingQueue.Status = JudgeQueueStatus.Pending;
                existingQueue.QueuedAt = DateTime.UtcNow;
                existingQueue.RetryCount = 0;
                existingQueue.ErrorMessage = null;
            }
            else
            {
                var judgeQueue = new JudgeQueue
                {
                    SubmissionId = submissionId,
                    Status = JudgeQueueStatus.Pending,
                    QueuedAt = DateTime.UtcNow,
                    Priority = 0
                };
                _context.JudgeQueues.Add(judgeQueue);
            }

            await _context.SaveChangesAsync();
            Message = $"Submission {submissionId} has been queued for rejudging";
            return Page();
        }

        public async Task<IActionResult> OnPostRejudgeProblemAsync(int problemId)
        {
            var problem = await _context.Problems.FindAsync(problemId);
            if (problem == null)
            {
                Message = "Problem not found";
                return Page();
            }

            var submissions = await _context.Submissions
                .Where(s => s.ProblemId == problemId)
                .ToListAsync();

            foreach (var submission in submissions)
            {
                // Reset submission status
                submission.Status = SubmissionStatus.Queueing;
                submission.Score = 0;
                submission.ExecutionTimeMs = 0;
                submission.CompileError = null;
                submission.RuntimeError = null;
                submission.JudgedAt = null;

                // Clear existing results
                var existingResults = await _context.SubmissionResults
                    .Where(sr => sr.SubmissionId == submission.Id)
                    .ToListAsync();
                _context.SubmissionResults.RemoveRange(existingResults);

                // Add to judge queue
                var existingQueue = await _context.JudgeQueues
                    .FirstOrDefaultAsync(jq => jq.SubmissionId == submission.Id);
                
                if (existingQueue != null)
                {
                    existingQueue.Status = JudgeQueueStatus.Pending;
                    existingQueue.QueuedAt = DateTime.UtcNow;
                    existingQueue.RetryCount = 0;
                    existingQueue.ErrorMessage = null;
                }
                else
                {
                    var judgeQueue = new JudgeQueue
                    {
                        SubmissionId = submission.Id,
                        Status = JudgeQueueStatus.Pending,
                        QueuedAt = DateTime.UtcNow,
                        Priority = 0
                    };
                    _context.JudgeQueues.Add(judgeQueue);
                }
            }

            await _context.SaveChangesAsync();
            Message = $"{submissions.Count} submissions for problem {problemId} have been queued for rejudging";
            return Page();
        }

        public async Task<IActionResult> OnPostRejudgeContestAsync(int contestId)
        {
            var contest = await _context.Contests.FindAsync(contestId);
            if (contest == null)
            {
                Message = "Contest not found";
                return Page();
            }

            var submissions = await _context.Submissions
                .Where(s => s.ContestId == contestId)
                .ToListAsync();

            foreach (var submission in submissions)
            {
                // Reset submission status
                submission.Status = SubmissionStatus.Queueing;
                submission.Score = 0;
                submission.ExecutionTimeMs = 0;
                submission.CompileError = null;
                submission.RuntimeError = null;
                submission.JudgedAt = null;

                // Clear existing results
                var existingResults = await _context.SubmissionResults
                    .Where(sr => sr.SubmissionId == submission.Id)
                    .ToListAsync();
                _context.SubmissionResults.RemoveRange(existingResults);

                // Add to judge queue
                var existingQueue = await _context.JudgeQueues
                    .FirstOrDefaultAsync(jq => jq.SubmissionId == submission.Id);
                
                if (existingQueue != null)
                {
                    existingQueue.Status = JudgeQueueStatus.Pending;
                    existingQueue.QueuedAt = DateTime.UtcNow;
                    existingQueue.RetryCount = 0;
                    existingQueue.ErrorMessage = null;
                }
                else
                {
                    var judgeQueue = new JudgeQueue
                    {
                        SubmissionId = submission.Id,
                        Status = JudgeQueueStatus.Pending,
                        QueuedAt = DateTime.UtcNow,
                        Priority = 0
                    };
                    _context.JudgeQueues.Add(judgeQueue);
                }
            }

            await _context.SaveChangesAsync();
            Message = $"{submissions.Count} submissions for contest {contestId} have been queued for rejudging";
            return Page();
        }
    }
}
