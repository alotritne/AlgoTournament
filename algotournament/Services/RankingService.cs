using algotournament.Data;
using algotournament.Models;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Services
{
    public class RankingService
    {
        private readonly ApplicationDbContext _context;

        public RankingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CalculateContestRankingsAsync(int contestId)
        {
            var contest = await _context.Contests
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Include(c => c.Submissions)
                    .ThenInclude(s => s.Results)
                .FirstOrDefaultAsync(c => c.Id == contestId);

            if (contest == null)
            {
                return;
            }

            // Clear existing rankings
            var existingRankings = await _context.Rankings
                .Where(r => r.ContestId == contestId)
                .ToListAsync();
            _context.Rankings.RemoveRange(existingRankings);

            var rankings = new List<Ranking>();

            foreach (var participant in contest.Participants)
            {
                var userSubmissions = contest.Submissions
                    .Where(s => s.UserId == participant.UserId)
                    .ToList();

                var solvedProblems = new HashSet<int>();
                var totalScore = 0;
                var penaltyTime = 0;

                if (contest.ScoringMode == ScoringMode.ACM)
                {
                    // ACM scoring: only count accepted submissions
                    var acceptedSubmissions = userSubmissions
                        .Where(s => s.Status == SubmissionStatus.Accepted)
                        .GroupBy(s => s.ProblemId)
                        .Select(g => g.OrderBy(s => s.SubmittedAt).First());

                    foreach (var submission in acceptedSubmissions)
                    {
                        solvedProblems.Add(submission.ProblemId);
                        totalScore += 100;

                        // Calculate penalty: 20 minutes per wrong submission + time of accepted submission
                        var wrongAttempts = userSubmissions
                            .Count(s => s.ProblemId == submission.ProblemId && s.Status != SubmissionStatus.Accepted && s.SubmittedAt < submission.SubmittedAt);
                        penaltyTime += wrongAttempts * 20 + (int)(submission.SubmittedAt - contest.StartTime).TotalMinutes;
                    }
                }
                else
                {
                    // OI scoring: partial scoring based on test cases
                    var problemScores = new Dictionary<int, int>();

                    foreach (var submission in userSubmissions)
                    {
                        if (!problemScores.ContainsKey(submission.ProblemId) || submission.Score > problemScores[submission.ProblemId])
                        {
                            problemScores[submission.ProblemId] = submission.Score;
                        }
                    }

                    foreach (var (problemId, score) in problemScores)
                    {
                        if (score == 100)
                        {
                            solvedProblems.Add(problemId);
                        }
                        totalScore += score;
                    }

                    // For OI, penalty is based on submission time
                    var lastSubmission = userSubmissions.Max(s => s.SubmittedAt);
                    penaltyTime = (int)(lastSubmission - contest.StartTime).TotalMinutes;
                }

                rankings.Add(new Ranking
                {
                    ContestId = contestId,
                    UserId = participant.UserId,
                    ProblemsSolved = solvedProblems.Count,
                    TotalScore = totalScore,
                    PenaltyTime = penaltyTime,
                    Rank = 0 // Will be calculated after sorting
                });
            }

            // Sort rankings: by score descending, then by penalty ascending, then by user handle
            var sortedRankings = rankings
                .OrderByDescending(r => r.TotalScore)
                .ThenBy(r => r.PenaltyTime)
                .ThenBy(r => r.User?.Handle)
                .ToList();

            // Assign ranks
            for (int i = 0; i < sortedRankings.Count; i++)
            {
                sortedRankings[i].Rank = i + 1;
            }

            await _context.Rankings.AddRangeAsync(sortedRankings);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Ranking>> GetContestRankingsAsync(int contestId)
        {
            return await _context.Rankings
                .Include(r => r.User)
                .Where(r => r.ContestId == contestId)
                .OrderBy(r => r.Rank)
                .ToListAsync();
        }
    }
}
