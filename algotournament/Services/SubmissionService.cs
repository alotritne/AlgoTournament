using algotournament.Data;
using algotournament.Models;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Services
{
    public class SubmissionService
    {
        private readonly ApplicationDbContext _context;

        public SubmissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Submission> CreateSubmissionAsync(string userId, int problemId, string sourceCode, ProgrammingLanguage language, int? contestId = null, int? duelMatchId = null)
        {
            if (contestId.HasValue && duelMatchId.HasValue)
            {
                throw new InvalidOperationException("Submission cannot belong to both a contest and a duel match.");
            }

            var submission = new Submission
            {
                UserId = userId,
                ProblemId = problemId,
                SourceCode = sourceCode,
                Language = language,
                Status = SubmissionStatus.Queueing,
                SubmittedAt = DateTime.UtcNow,
                ContestId = contestId,
                DuelMatchId = duelMatchId
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

            return submission;
        }

        public async Task<Submission?> GetSubmissionAsync(int submissionId)
        {
            return await _context.Submissions
                .Include(s => s.User)
                .Include(s => s.Problem)
                .Include(s => s.Contest)
                .Include(s => s.Results)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }

        public async Task<List<Submission>> GetUserSubmissionsAsync(string userId, int limit = 50)
        {
            return await _context.Submissions
                .Include(s => s.Problem)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Submission>> GetProblemSubmissionsAsync(int problemId, int limit = 50)
        {
            return await _context.Submissions
                .Include(s => s.User)
                .Where(s => s.ProblemId == problemId)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Submission>> GetContestSubmissionsAsync(int contestId, int limit = 100)
        {
            return await _context.Submissions
                .Include(s => s.User)
                .Include(s => s.Problem)
                .Where(s => s.ContestId == contestId)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task UpdateSubmissionStatusAsync(int submissionId, SubmissionStatus status, int score = 0, int executionTimeMs = 0, string? compileError = null, string? runtimeError = null)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission != null)
            {
                submission.Status = status;
                submission.Score = score;
                submission.ExecutionTimeMs = executionTimeMs;
                submission.CompileError = compileError;
                submission.RuntimeError = runtimeError;
                submission.JudgedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveSubmissionResultAsync(int submissionId, int testCaseId, SubmissionStatus status, int executionTimeMs = 0, int memoryUsedKB = 0, string? output = null, string? errorMessage = null)
        {
            var result = new SubmissionResult
            {
                SubmissionId = submissionId,
                TestCaseId = testCaseId,
                Status = status,
                ExecutionTimeMs = executionTimeMs,
                MemoryUsedKB = memoryUsedKB,
                Output = output,
                ErrorMessage = errorMessage
            };

            _context.SubmissionResults.Add(result);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Submission>> GetRecentSubmissionsAsync(int limit = 20)
        {
            return await _context.Submissions
                .Include(s => s.User)
                .Include(s => s.Problem)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}
