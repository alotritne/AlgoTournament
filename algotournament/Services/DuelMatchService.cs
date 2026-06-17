using System.Collections.Concurrent;
using algotournament.Data;
using algotournament.Hubs;
using algotournament.Models;
using algotournament.Models.Dtos;
using algotournament.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace algotournament.Services
{
    public class DuelMatchService : IDuelJudgeNotifier
    {
        private readonly ApplicationDbContext _context;
        private readonly DuelScoreService _scoreService;
        private readonly IHubContext<DuelHub> _hubContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DuelMatchService> _logger;
        private static readonly ConcurrentDictionary<string, byte> CountdownInProgress = new();
        private static readonly ConcurrentDictionary<string, HashSet<string>> ConnectedUsers = new();
        private static readonly ConcurrentDictionary<string, (string RoomCode, string UserId)> ConnectionRooms = new();
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> PendingDisconnectLeaves = new();
        private static readonly TimeSpan DisconnectLeaveDelay = TimeSpan.FromSeconds(3);

        public DuelMatchService(
            ApplicationDbContext context,
            DuelScoreService scoreService,
            IHubContext<DuelHub> hubContext,
            IServiceScopeFactory scopeFactory,
            ILogger<DuelMatchService> logger)
        {
            _context = context;
            _scoreService = scoreService;
            _hubContext = hubContext;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public static void TrackConnection(string roomCode, string userId, bool connected)
        {
            roomCode = DuelService.NormalizeRoomCode(roomCode);
            var set = ConnectedUsers.GetOrAdd(roomCode, _ => new HashSet<string>(StringComparer.Ordinal));
            lock (set)
            {
                if (connected) set.Add(userId);
                else set.Remove(userId);
            }
        }

        public static bool IsUserConnected(string roomCode, string userId)
        {
            roomCode = DuelService.NormalizeRoomCode(roomCode);
            if (!ConnectedUsers.TryGetValue(roomCode, out var set)) return false;
            lock (set) return set.Contains(userId);
        }

        public static void TrackConnectionRoom(string connectionId, string roomCode, string userId)
        {
            roomCode = DuelService.NormalizeRoomCode(roomCode);
            ConnectionRooms[connectionId] = (roomCode, userId);
            CancelPendingDisconnectLeave(roomCode, userId);
        }

        public static bool TryRemoveConnectionRoom(string connectionId, out string roomCode, out string userId)
        {
            roomCode = string.Empty;
            userId = string.Empty;
            if (!ConnectionRooms.TryRemove(connectionId, out var info)) return false;
            roomCode = info.RoomCode;
            userId = info.UserId;
            return true;
        }

        public static void UntrackConnectionRoom(string connectionId)
        {
            ConnectionRooms.TryRemove(connectionId, out _);
        }

        public static void CancelPendingDisconnectLeave(string roomCode, string userId)
        {
            roomCode = DuelService.NormalizeRoomCode(roomCode);
            var key = GetDisconnectLeaveKey(roomCode, userId);
            if (PendingDisconnectLeaves.TryRemove(key, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }

        public void ScheduleDisconnectLeave(string roomCode, string userId)
        {
            roomCode = DuelService.NormalizeRoomCode(roomCode);
            var key = GetDisconnectLeaveKey(roomCode, userId);
            if (PendingDisconnectLeaves.TryRemove(key, out var existing))
            {
                existing.Cancel();
                existing.Dispose();
            }

            var cts = new CancellationTokenSource();
            PendingDisconnectLeaves[key] = cts;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(DisconnectLeaveDelay, cts.Token);
                    PendingDisconnectLeaves.TryRemove(key, out _);

                    using var scope = _scopeFactory.CreateScope();
                    var duelService = scope.ServiceProvider.GetRequiredService<DuelService>();
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<DuelHub>>();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    try
                    {
                        await duelService.LeaveRoomAsync(userId, roomCode);
                    }
                    catch (InvalidOperationException)
                    {
                        return;
                    }

                    var user = await context.Users.FindAsync(userId);
                    await hubContext.Clients.Group(DuelHub.GroupName(roomCode)).SendAsync("PlayerLeft", new
                    {
                        roomCode,
                        userId,
                        handle = user?.Handle
                    });
                    await duelService.BroadcastLobbyUpdatedAsync(roomCode);
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Scheduled disconnect leave failed for room {RoomCode}, user {UserId}", roomCode, userId);
                }
                finally
                {
                    cts.Dispose();
                }
            });
        }

        private static string GetDisconnectLeaveKey(string roomCode, string userId) =>
            $"{DuelService.NormalizeRoomCode(roomCode)}:{userId}";

        public async Task TryStartCountdownAsync(string roomCode)
        {
            roomCode = DuelService.NormalizeRoomCode(roomCode);
            if (!CountdownInProgress.TryAdd(roomCode, 0))
            {
                return;
            }

            try
            {
                var room = await _context.DuelRooms
                    .Include(r => r.Participants)
                    .FirstOrDefaultAsync(r => r.RoomCode == roomCode);

                if (room == null || room.Status != DuelRoomStatus.Ready ||
                    room.Participants.Count < room.MaxPlayers ||
                    !room.Participants.All(p => p.IsReady))
                {
                    return;
                }

                await _hubContext.Clients.Group(DuelHub.GroupName(roomCode))
                    .SendAsync("CountdownStarted", new { roomCode, seconds = 3 });

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(3000);
                        using var scope = _scopeFactory.CreateScope();
                        var matchService = scope.ServiceProvider.GetRequiredService<DuelMatchService>();
                        await matchService.StartMatchAsync(roomCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to start match after countdown for room {RoomCode}", roomCode);
                    }
                    finally
                    {
                        CountdownInProgress.TryRemove(roomCode, out _);
                    }
                });
            }
            catch
            {
                CountdownInProgress.TryRemove(roomCode, out _);
                throw;
            }
        }

        public async Task<DuelMatch?> StartMatchAsync(string roomCode)
        {
            roomCode = DuelService.NormalizeRoomCode(roomCode);
            var room = await _context.DuelRooms
                .Include(r => r.Participants)
                .Include(r => r.Match)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);

            if (room == null || room.Match != null ||
                room.Status is DuelRoomStatus.InProgress or DuelRoomStatus.Finished or DuelRoomStatus.Cancelled)
            {
                return room?.Match;
            }

            if (room.Participants.Count < 2)
            {
                return null;
            }

            var p1 = room.Participants.First(p => p.SlotIndex == 1);
            var p2 = room.Participants.OrderBy(p => p.SlotIndex).Skip(1).First();
            var now = DateTime.UtcNow;

            var match = new DuelMatch
            {
                DuelRoomId = room.Id,
                StartedAt = now,
                DurationMinutes = room.DurationMinutes,
                Status = DuelMatchStatus.InProgress,
                Player1UserId = p1.UserId,
                Player2UserId = p2.UserId
            };

            room.Status = DuelRoomStatus.InProgress;
            _context.DuelMatches.Add(match);
            await _context.SaveChangesAsync();

            var endsAt = now.AddMinutes(room.DurationMinutes);
            await _hubContext.Clients.Group(DuelHub.GroupName(roomCode))
                .SendAsync("MatchStarted", new
                {
                    roomCode,
                    matchId = match.Id,
                    startedAt = DateTime.SpecifyKind(now, DateTimeKind.Utc),
                    endsAt = DateTime.SpecifyKind(endsAt, DateTimeKind.Utc),
                    durationMinutes = room.DurationMinutes,
                    problemId = room.ProblemId
                });

            return match;
        }

        public async Task ValidateSubmissionAsync(string userId, int duelMatchId)
        {
            var match = await _context.DuelMatches
                .Include(m => m.Room)
                .FirstOrDefaultAsync(m => m.Id == duelMatchId);

            if (match == null || match.Status != DuelMatchStatus.InProgress)
            {
                throw new InvalidOperationException("Match is not in progress.");
            }

            if (match.StartedAt.AddMinutes(match.DurationMinutes) < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Match time has expired.");
            }

            if (match.Player1UserId != userId && match.Player2UserId != userId)
            {
                throw new InvalidOperationException("You are not a participant in this match.");
            }
        }

        public async Task OnSubmissionCreatedAsync(int submissionId)
        {
            var submission = await _context.Submissions
                .Include(s => s.User)
                .Include(s => s.DuelMatch)
                    .ThenInclude(m => m!.Room)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission?.DuelMatchId == null || submission.DuelMatch == null) return;
            if (submission.DuelMatch.Status != DuelMatchStatus.InProgress) return;

            var publicStatus = MapPublicStatus(submission.Status);
            var count = await _context.Submissions.CountAsync(s =>
                s.DuelMatchId == submission.DuelMatchId && s.UserId == submission.UserId);

            await BroadcastSubmissionUpdateAsync(submission.DuelMatch, submission.UserId, submission.User.Handle, publicStatus, count);
        }

        public async Task NotifySubmissionJudgedAsync(int submissionId)
        {
            await OnSubmissionJudgedAsync(submissionId);
        }

        public async Task OnSubmissionJudgedAsync(int submissionId)
        {
            var submission = await _context.Submissions
                .Include(s => s.User)
                .Include(s => s.DuelMatch)
                    .ThenInclude(m => m!.Room)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission?.DuelMatchId == null || submission.DuelMatch == null) return;

            var match = submission.DuelMatch;
            if (match.Status != DuelMatchStatus.InProgress) return;

            var roomCode = match.Room.RoomCode;
            var publicStatus = MapPublicStatus(submission.Status);
            var count = await _context.Submissions.CountAsync(s =>
                s.DuelMatchId == match.Id && s.UserId == submission.UserId);

            await BroadcastSubmissionUpdateAsync(match, submission.UserId, submission.User.Handle, publicStatus, count);

            if (submission.Status == SubmissionStatus.Accepted)
            {
                await _hubContext.Clients.Group(DuelHub.GroupName(roomCode))
                    .SendAsync("PlayerAccepted", new
                    {
                        matchId = match.Id,
                        userId = submission.UserId,
                        handle = submission.User.Handle,
                        acceptedAt = submission.JudgedAt,
                        runtimeMs = submission.ExecutionTimeMs,
                        isFirst = true
                    });

                await EndMatchAsync(match.Id, DuelMatchStatus.Finished, submission.UserId, solvedFirst: true);
            }
        }

        public async Task EndMatchAsync(
            int matchId,
            DuelMatchStatus endStatus = DuelMatchStatus.Finished,
            string? winnerUserId = null,
            bool resolveWinnerOnTimeout = false,
            bool solvedFirst = false)
        {
            var match = await _context.DuelMatches
                .Include(m => m.Room)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null || match.Status != DuelMatchStatus.InProgress) return;

            if (resolveWinnerOnTimeout)
            {
                winnerUserId = await _scoreService.DetermineWinnerByAcceptedTimeAsync(matchId);
                solvedFirst = false;
            }

            string? winnerHandle = null;
            if (winnerUserId != null)
            {
                winnerHandle = await _context.Users
                    .Where(u => u.Id == winnerUserId)
                    .Select(u => u.Handle)
                    .FirstOrDefaultAsync();
            }

            match.WinnerUserId = winnerUserId;
            match.FinalScorePlayer1 = winnerUserId == match.Player1UserId ? 1 : 0;
            match.FinalScorePlayer2 = winnerUserId == match.Player2UserId ? 1 : 0;
            match.Status = endStatus;
            match.EndedAt = DateTime.UtcNow;
            match.Room.Status = DuelRoomStatus.Finished;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(DuelHub.GroupName(match.Room.RoomCode))
                .SendAsync("MatchFinished", new
                {
                    matchId = match.Id,
                    roomCode = match.Room.RoomCode,
                    winnerUserId,
                    winnerHandle,
                    isDraw = winnerUserId == null,
                    solvedFirst,
                    redirectUrl = $"/Duels/Result/{match.Id}"
                });
        }

        public async Task ForceEndMatchAsync(int matchId)
        {
            await EndMatchAsync(matchId, DuelMatchStatus.ForceEnded, resolveWinnerOnTimeout: true);
        }

        public async Task DeleteMatchAsync(int matchId)
        {
            var match = await _context.DuelMatches
                .Include(m => m.Submissions)
                .Include(m => m.Room)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null) return;

            foreach (var submission in match.Submissions)
            {
                submission.DuelMatchId = null;
            }

            match.Room.Status = DuelRoomStatus.Cancelled;
            _context.DuelMatches.Remove(match);
            await _context.SaveChangesAsync();
        }

        public async Task<DuelMatchStateDto?> GetMatchStateAsync(string roomCode, string userId)
        {
            roomCode = DuelService.NormalizeRoomCode(roomCode);
            var room = await _context.DuelRooms
                .Include(r => r.Problem)
                .Include(r => r.Match)
                .Include(r => r.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);

            if (room?.Match == null) return null;

            var match = room.Match;
            var p1 = room.Participants.FirstOrDefault(p => p.SlotIndex == 1);
            var p2 = room.Participants.FirstOrDefault(p => p.SlotIndex == 2);
            if (p1 == null || p2 == null) return null;

            var mySlot = p1.UserId == userId ? (byte)1 : (byte)2;
            var opponentId = mySlot == 1 ? match.Player2UserId : match.Player1UserId;

            var startedAtUtc = DateTime.SpecifyKind(match.StartedAt, DateTimeKind.Utc);
            var endsAtUtc = startedAtUtc.AddMinutes(match.DurationMinutes);

            var myLast = await _context.Submissions
                .Where(s => s.DuelMatchId == match.Id && s.UserId == userId)
                .OrderByDescending(s => s.SubmittedAt)
                .FirstOrDefaultAsync();

            var oppLast = await _context.Submissions
                .Where(s => s.DuelMatchId == match.Id && s.UserId == opponentId)
                .OrderByDescending(s => s.SubmittedAt)
                .FirstOrDefaultAsync();

            return new DuelMatchStateDto
            {
                MatchId = match.Id,
                RoomCode = room.RoomCode,
                ProblemId = room.ProblemId,
                StartedAt = startedAtUtc,
                EndsAt = endsAtUtc,
                DurationMinutes = match.DurationMinutes,
                Status = match.Status,
                Player1 = MapParticipant(p1),
                Player2 = MapParticipant(p2),
                CurrentUserId = userId,
                CurrentUserSlot = mySlot,
                MyLastStatus = myLast != null ? MapPublicStatus(myLast.Status) : null,
                OpponentLastStatus = oppLast != null ? MapPublicStatus(oppLast.Status) : null,
                MySubmissionCount = await _context.Submissions.CountAsync(s => s.DuelMatchId == match.Id && s.UserId == userId),
                OpponentSubmissionCount = await _context.Submissions.CountAsync(s => s.DuelMatchId == match.Id && s.UserId == opponentId)
            };
        }

        public async Task ExpireMatchesAsync()
        {
            var now = DateTime.UtcNow;
            var expired = await _context.DuelMatches
                .Where(m => m.Status == DuelMatchStatus.InProgress &&
                    m.StartedAt.AddMinutes(m.DurationMinutes) <= now)
                .Select(m => m.Id)
                .ToListAsync();

            foreach (var matchId in expired)
            {
                await EndMatchAsync(matchId, DuelMatchStatus.Finished, resolveWinnerOnTimeout: true);
            }
        }

        private async Task BroadcastSubmissionUpdateAsync(DuelMatch match, string userId, string handle, string publicStatus, int submissionCount)
        {
            await _hubContext.Clients.Group(DuelHub.GroupName(match.Room.RoomCode))
                .SendAsync("SubmissionStatusChanged", new
                {
                    matchId = match.Id,
                    userId,
                    handle,
                    status = publicStatus,
                    submissionCount
                });
        }

        private static DuelParticipantDto MapParticipant(DuelParticipant p) => new()
        {
            UserId = p.UserId,
            Handle = p.User.Handle,
            Rating = p.User.Rating,
            IsReady = p.IsReady,
            SlotIndex = p.SlotIndex,
            IsConnected = false
        };

        public static string MapPublicStatus(SubmissionStatus status) => status switch
        {
            SubmissionStatus.Pending or SubmissionStatus.Queueing => DuelPublicSubmissionStatus.Submitting,
            SubmissionStatus.Compiling => DuelPublicSubmissionStatus.Compiling,
            SubmissionStatus.Running => DuelPublicSubmissionStatus.Running,
            SubmissionStatus.Accepted => DuelPublicSubmissionStatus.Accepted,
            _ => DuelPublicSubmissionStatus.NotAccepted
        };
    }
}
