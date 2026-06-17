using algotournament.Data;
using algotournament.Hubs;
using algotournament.Models;
using algotournament.Models.Dtos;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Services
{
    public class DuelService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<DuelHub> _hubContext;
        private readonly ILogger<DuelService> _logger;
        private static readonly char[] RoomCodeChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        private const int RoomCodeLength = 6;
        private const int RoomExpiryMinutes = 60;

        public DuelService(ApplicationDbContext context, IHubContext<DuelHub> hubContext, ILogger<DuelService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<string> GenerateUniqueRoomCodeAsync()
        {
            for (var attempt = 0; attempt < 50; attempt++)
            {
                var code = GenerateRoomCode();
                var exists = await _context.DuelRooms.AnyAsync(r => r.RoomCode == code);
                if (!exists) return code;
            }

            throw new InvalidOperationException("Unable to generate unique room code.");
        }

        public async Task<DuelRoom> CreateRoomAsync(string userId, int problemId, int durationMinutes)
        {
            var problem = await _context.Problems.FirstOrDefaultAsync(p => p.Id == problemId && p.IsPublic);
            if (problem == null)
            {
                throw new InvalidOperationException("Problem not found or not available.");
            }

            if (!new[] { 10, 15, 30, 60 }.Contains(durationMinutes))
            {
                throw new InvalidOperationException("Invalid duration.");
            }

            var roomCode = await GenerateUniqueRoomCodeAsync();
            var now = DateTime.UtcNow;

            var room = new DuelRoom
            {
                RoomCode = roomCode,
                ProblemId = problemId,
                HostUserId = userId,
                MaxPlayers = 2,
                DurationMinutes = durationMinutes,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(RoomExpiryMinutes),
                Status = DuelRoomStatus.Waiting
            };

            _context.DuelRooms.Add(room);
            await _context.SaveChangesAsync();

            _context.DuelParticipants.Add(new DuelParticipant
            {
                DuelRoomId = room.Id,
                UserId = userId,
                IsReady = false,
                JoinedAt = now,
                SlotIndex = 1
            });
            await _context.SaveChangesAsync();

            return room;
        }

        public async Task<DuelRoom> JoinRoomAsync(string userId, string roomCode)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = await _context.DuelRooms
                .Include(r => r.Participants)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);

            if (room == null)
            {
                throw new InvalidOperationException("Room not found.");
            }

            if (room.Status is DuelRoomStatus.InProgress or DuelRoomStatus.Finished or DuelRoomStatus.Cancelled)
            {
                throw new InvalidOperationException("This duel has already started or ended.");
            }

            if (room.ExpiresAt < DateTime.UtcNow)
            {
                room.Status = DuelRoomStatus.Cancelled;
                await _context.SaveChangesAsync();
                throw new InvalidOperationException("Room has expired.");
            }

            var existing = room.Participants.FirstOrDefault(p => p.UserId == userId);
            if (existing != null)
            {
                return room;
            }

            if (room.Participants.Count >= room.MaxPlayers)
            {
                throw new InvalidOperationException("Room is full.");
            }

            _context.DuelParticipants.Add(new DuelParticipant
            {
                DuelRoomId = room.Id,
                UserId = userId,
                IsReady = false,
                JoinedAt = DateTime.UtcNow,
                SlotIndex = 2
            });
            await _context.SaveChangesAsync();

            await BroadcastLobbyUpdatedAsync(roomCode);

            return room;
        }

        public async Task BroadcastLobbyUpdatedAsync(string roomCode)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var lobby = await GetLobbyStateAsync(roomCode);
            if (lobby == null) return;

            await _hubContext.Clients.Group(DuelHub.GroupName(roomCode))
                .SendAsync("LobbyUpdated", lobby);
        }

        public async Task LeaveRoomAsync(string userId, string roomCode)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = await _context.DuelRooms
                .Include(r => r.Participants)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);

            if (room == null) return;

            if (room.Status is DuelRoomStatus.InProgress or DuelRoomStatus.Finished)
            {
                throw new InvalidOperationException("Cannot leave an active or finished duel.");
            }

            var participant = room.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null) return;

            _context.DuelParticipants.Remove(participant);

            if (room.HostUserId == userId || room.Participants.Count <= 1)
            {
                room.Status = DuelRoomStatus.Cancelled;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<DuelLobbyStateDto?> GetLobbyStateAsync(string roomCode, string? currentUserId = null)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = await _context.DuelRooms
                .Include(r => r.Problem)
                .Include(r => r.Participants)
                    .ThenInclude(p => p.User)
                .Include(r => r.Match)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);

            if (room == null) return null;

            return new DuelLobbyStateDto
            {
                RoomCode = room.RoomCode,
                ProblemId = room.ProblemId,
                ProblemTitle = room.Problem.TitleVi,
                DurationMinutes = room.DurationMinutes,
                Status = room.Status,
                MatchId = room.Match?.Id,
                CurrentUserId = currentUserId,
                Participants = room.Participants
                    .OrderBy(p => p.SlotIndex)
                    .Select(p => new DuelParticipantDto
                    {
                        UserId = p.UserId,
                        Handle = p.User.Handle,
                        Rating = p.User.Rating,
                        IsReady = p.IsReady,
                        SlotIndex = p.SlotIndex,
                        IsConnected = false
                    }).ToList()
            };
        }

        public async Task<bool> SetReadyAsync(string userId, string roomCode, bool isReady)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = await _context.DuelRooms
                .Include(r => r.Participants)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);

            if (room == null) return false;

            if (room.Status is DuelRoomStatus.InProgress or DuelRoomStatus.Finished or DuelRoomStatus.Cancelled)
            {
                return false;
            }

            var participant = room.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null) return false;

            participant.IsReady = isReady;

            if (room.Participants.Count == room.MaxPlayers &&
                room.Participants.All(p => p.IsReady))
            {
                room.Status = DuelRoomStatus.Ready;
            }
            else if (room.Status == DuelRoomStatus.Ready && !room.Participants.All(p => p.IsReady))
            {
                room.Status = DuelRoomStatus.Waiting;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ExpireStaleRoomsAsync()
        {
            var now = DateTime.UtcNow;
            var stale = await _context.DuelRooms
                .Where(r => (r.Status == DuelRoomStatus.Waiting || r.Status == DuelRoomStatus.Ready)
                    && r.ExpiresAt < now)
                .ToListAsync();

            foreach (var room in stale)
            {
                room.Status = DuelRoomStatus.Cancelled;
            }

            if (stale.Count > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cancelled {Count} stale duel rooms", stale.Count);
            }
        }

        public static string NormalizeRoomCode(string roomCode) =>
            roomCode.Trim().ToUpperInvariant();

        private static string GenerateRoomCode()
        {
            var chars = new char[RoomCodeLength];
            for (var i = 0; i < RoomCodeLength; i++)
            {
                chars[i] = RoomCodeChars[Random.Shared.Next(RoomCodeChars.Length)];
            }
            return new string(chars);
        }
    }
}
