using algotournament.Models.Dtos;
using algotournament.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Hubs
{
    [Authorize]
    public class DuelHub : Hub
    {
        private readonly DuelService _duelService;
        private readonly DuelMatchService _matchService;
        private readonly algotournament.Data.ApplicationDbContext _context;

        public DuelHub(DuelService duelService, DuelMatchService matchService, algotournament.Data.ApplicationDbContext context)
        {
            _duelService = duelService;
            _matchService = matchService;
            _context = context;
        }

        public static string GroupName(string roomCode) => $"room:{DuelService.NormalizeRoomCode(roomCode)}";

        public async Task JoinRoomGroup(string roomCode)
        {
            var userId = GetUserId();
            if (userId == null) throw new HubException("Unauthorized");

            roomCode = DuelService.NormalizeRoomCode(roomCode);
            var isParticipant = await _context.DuelParticipants
                .Include(p => p.Room)
                .AnyAsync(p => p.UserId == userId && p.Room!.RoomCode == roomCode);

            if (!isParticipant)
            {
                throw new HubException("Not a participant in this room.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(roomCode));
            DuelMatchService.TrackConnection(roomCode, userId, true);
            DuelMatchService.TrackConnectionRoom(Context.ConnectionId, roomCode, userId);

            var user = await _context.Users.FindAsync(userId);
            var participant = await _context.DuelParticipants
                .Include(p => p.Room)
                .FirstAsync(p => p.UserId == userId && p.Room!.RoomCode == roomCode);

            await Clients.Group(GroupName(roomCode)).SendAsync("PlayerJoined", new
            {
                roomCode,
                userId,
                handle = user?.Handle,
                rating = user?.Rating ?? 0,
                slotIndex = participant.SlotIndex,
                connected = true
            });

            await _duelService.BroadcastLobbyUpdatedAsync(roomCode);
        }

        public async Task LeaveRoomGroup(string roomCode)
        {
            var userId = GetUserId();
            if (userId == null) return;

            roomCode = DuelService.NormalizeRoomCode(roomCode);
            DuelMatchService.CancelPendingDisconnectLeave(roomCode, userId);
            DuelMatchService.UntrackConnectionRoom(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(roomCode));
            DuelMatchService.TrackConnection(roomCode, userId, false);

            try
            {
                await _duelService.LeaveRoomAsync(userId, roomCode);
            }
            catch (InvalidOperationException)
            {
                return;
            }

            var user = await _context.Users.FindAsync(userId);
            await Clients.Group(GroupName(roomCode)).SendAsync("PlayerLeft", new
            {
                roomCode,
                userId,
                handle = user?.Handle
            });
            await _duelService.BroadcastLobbyUpdatedAsync(roomCode);
        }

        public async Task SetReady(string roomCode, bool isReady)
        {
            var userId = GetUserId();
            if (userId == null) throw new HubException("Unauthorized");

            roomCode = DuelService.NormalizeRoomCode(roomCode);
            var updated = await _duelService.SetReadyAsync(userId, roomCode, isReady);
            if (!updated) throw new HubException("Unable to update ready state.");

            var lobby = await _duelService.GetLobbyStateAsync(roomCode, userId);
            var readyCount = lobby?.Participants.Count(p => p.IsReady) ?? 0;

            await Clients.Group(GroupName(roomCode)).SendAsync("PlayerReady", new
            {
                roomCode,
                userId,
                isReady,
                readyCount
            });

            if (lobby?.Status == Models.DuelRoomStatus.Ready && readyCount >= 2)
            {
                await _matchService.TryStartCountdownAsync(roomCode);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (DuelMatchService.TryRemoveConnectionRoom(Context.ConnectionId, out var roomCode, out var userId))
            {
                DuelMatchService.TrackConnection(roomCode, userId, false);
                _matchService.ScheduleDisconnectLeave(roomCode, userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private string? GetUserId() => Context.UserIdentifier;
    }
}
