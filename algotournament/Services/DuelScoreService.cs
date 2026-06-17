using algotournament.Data;
using algotournament.Models;
using algotournament.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Services
{
    public class DuelScoreService
    {
        private readonly ApplicationDbContext _context;

        public DuelScoreService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DuelParticipantStats> GetParticipantStatsAsync(int matchId, string userId)
        {
            var submissions = await _context.Submissions
                .Include(s => s.Results)
                .Where(s => s.DuelMatchId == matchId && s.UserId == userId)
                .OrderBy(s => s.SubmittedAt)
                .ToListAsync();

            var judged = submissions.Where(s => s.JudgedAt != null).ToList();
            var accepted = judged
                .Where(s => s.Status == SubmissionStatus.Accepted)
                .OrderBy(s => s.JudgedAt)
                .FirstOrDefault();

            var wrongAttempts = judged.Count(s => s.Status != SubmissionStatus.Accepted);

            int? memoryKb = null;
            if (accepted != null)
            {
                memoryKb = accepted.Results.Any()
                    ? accepted.Results.Max(r => r.MemoryUsedKB)
                    : accepted.MemoryUsedKB;
            }

            return new DuelParticipantStats
            {
                UserId = userId,
                SubmissionCount = submissions.Count,
                WrongAttempts = wrongAttempts,
                HasAccepted = accepted != null,
                AcceptedAt = accepted?.JudgedAt,
                BestRuntimeMs = accepted?.ExecutionTimeMs,
                BestMemoryKb = memoryKb
            };
        }

        /// <summary>
        /// Winner by earliest Accepted time. Null = draw (timeout with no AC, or tie on same ms).
        /// </summary>
        public async Task<string?> DetermineWinnerByAcceptedTimeAsync(int matchId)
        {
            var match = await _context.DuelMatches.AsNoTracking().FirstAsync(m => m.Id == matchId);
            var p1Stats = await GetParticipantStatsAsync(matchId, match.Player1UserId);
            var p2Stats = await GetParticipantStatsAsync(matchId, match.Player2UserId);

            if (!p1Stats.HasAccepted && !p2Stats.HasAccepted) return null;
            if (p1Stats.HasAccepted && !p2Stats.HasAccepted) return match.Player1UserId;
            if (p2Stats.HasAccepted && !p1Stats.HasAccepted) return match.Player2UserId;

            if (p1Stats.AcceptedAt!.Value < p2Stats.AcceptedAt!.Value) return match.Player1UserId;
            if (p2Stats.AcceptedAt!.Value < p1Stats.AcceptedAt!.Value) return match.Player2UserId;

            return null;
        }

        public async Task RecalculateMatchResultAsync(int matchId)
        {
            var match = await _context.DuelMatches.FirstOrDefaultAsync(m => m.Id == matchId);
            if (match == null) return;

            var winnerId = await DetermineWinnerByAcceptedTimeAsync(matchId);
            match.WinnerUserId = winnerId;
            match.FinalScorePlayer1 = winnerId == match.Player1UserId ? 1 : 0;
            match.FinalScorePlayer2 = winnerId == match.Player2UserId ? 1 : 0;
            await _context.SaveChangesAsync();
        }

        public async Task<DuelResultDto> BuildResultDtoAsync(int matchId, string currentUserId)
        {
            var match = await _context.DuelMatches
                .Include(m => m.Room)
                    .ThenInclude(r => r.Problem)
                .Include(m => m.WinnerUser)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null) throw new InvalidOperationException("Match not found");

            var users = await _context.Users
                .Where(u => u.Id == match.Player1UserId || u.Id == match.Player2UserId)
                .ToListAsync();

            var p1User = users.First(u => u.Id == match.Player1UserId);
            var p2User = users.First(u => u.Id == match.Player2UserId);
            var p1Stats = await GetParticipantStatsAsync(matchId, match.Player1UserId);
            var p2Stats = await GetParticipantStatsAsync(matchId, match.Player2UserId);

            var player1 = MapPlayerResult(p1User, p1Stats, match.WinnerUserId);
            var player2 = MapPlayerResult(p2User, p2Stats, match.WinnerUserId);

            return new DuelResultDto
            {
                MatchId = match.Id,
                RoomCode = match.Room.RoomCode,
                ProblemTitle = match.Room.Problem.TitleVi,
                IsDraw = match.WinnerUserId == null,
                WinnerUserId = match.WinnerUserId,
                WinnerHandle = match.WinnerUser?.Handle,
                LoserHandle = match.WinnerUserId == match.Player1UserId ? p2User.Handle
                    : match.WinnerUserId == match.Player2UserId ? p1User.Handle : null,
                Player1 = player1,
                Player2 = player2,
                CurrentUserId = currentUserId
            };
        }

        private static DuelPlayerResultDto MapPlayerResult(ApplicationUser user, DuelParticipantStats stats, string? winnerUserId)
        {
            return new DuelPlayerResultDto
            {
                UserId = user.Id,
                Handle = user.Handle,
                RuntimeMs = stats.BestRuntimeMs,
                MemoryKb = stats.BestMemoryKb,
                WrongAttempts = stats.WrongAttempts,
                SubmissionCount = stats.SubmissionCount,
                AcceptedAt = stats.AcceptedAt,
                IsWinner = winnerUserId == user.Id,
                SolvedFirst = winnerUserId == user.Id && stats.HasAccepted
            };
        }
    }

    public class DuelParticipantStats
    {
        public string UserId { get; set; } = string.Empty;
        public int SubmissionCount { get; set; }
        public int WrongAttempts { get; set; }
        public bool HasAccepted { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public int? BestRuntimeMs { get; set; }
        public int? BestMemoryKb { get; set; }
    }
}
