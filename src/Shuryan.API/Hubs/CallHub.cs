using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.API.Hubs
{
    [Authorize]
    public class CallHub : Hub
    {
        private readonly ICallSessionService _sessionService;
        private readonly ILogger<CallHub> _logger;

        private const string RoomIdKey = "roomId";

        public CallHub(ICallSessionService sessionService, ILogger<CallHub> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            var roomId = Context.GetHttpContext()?.Request.Query["roomId"].ToString();

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("CallHub: rejected connection — invalid or missing JWT user claim");
                Context.Abort();
                return;
            }

            if (string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogWarning("CallHub: rejected connection for user {UserId} — missing roomId query parameter", userId);
                Context.Abort();
                return;
            }

            var session = await _sessionService.GetSessionByRoomIdAsync(roomId);

            if (session == null)
            {
                _logger.LogWarning("CallHub: rejected connection — room {RoomId} not found (user {UserId})", roomId, userId);
                Context.Abort();
                return;
            }

            bool joined;

            if (session.DoctorId == userId)
            {
                joined = await _sessionService.DoctorJoinedAsync(roomId, userId, Context.ConnectionId);
            }
            else if (session.PatientId == userId)
            {
                joined = await _sessionService.PatientJoinedAsync(roomId, userId, Context.ConnectionId);
            }
            else
            {
                _logger.LogWarning("CallHub: user {UserId} is not a participant of room {RoomId}", userId, roomId);
                Context.Abort();
                return;
            }

            if (!joined)
            {
                _logger.LogWarning("CallHub: join failed for user {UserId} in room {RoomId}", userId, roomId);
                Context.Abort();
                return;
            }

            Context.Items[RoomIdKey] = roomId;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            await Clients.OthersInGroup(roomId).SendAsync("ParticipantJoined", new { userId });

            var updated = await _sessionService.GetSessionByRoomIdAsync(roomId);
            if (updated?.Status == TelemedicineSessionStatus.Active)
            {
                await Clients.Group(roomId).SendAsync("CallReady", new
                {
                    roomId,
                    startedAt = updated.StartedAt
                });
            }

            _logger.LogInformation(
                "User {UserId} connected to room {RoomId} — connectionId {ConnectionId}",
                userId, roomId, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            var roomId = Context.Items[RoomIdKey]?.ToString();

            if (!string.IsNullOrEmpty(roomId))
            {
                await Clients.OthersInGroup(roomId).SendAsync("ParticipantLeft", new { userId });
                await _sessionService.HandleDisconnectAsync(Context.ConnectionId);
            }

            if (exception != null)
                _logger.LogError(exception, "CallHub: user {UserId} disconnected with error", userId);
            else
                _logger.LogInformation("CallHub: user {UserId} disconnected normally", userId);

            await base.OnDisconnectedAsync(exception);
        }

        #region WebRTC Signaling Methods

        public async Task SendOffer(string roomId, object sdpOffer)
        {
            if (!await IsParticipantAsync(roomId))
            {
                await Clients.Caller.SendAsync("Error", "You are not a participant of this room");
                return;
            }

            _logger.LogDebug("CallHub: relaying SDP offer in room {RoomId}", roomId);
            await Clients.OthersInGroup(roomId).SendAsync("ReceiveOffer", sdpOffer);
        }

        public async Task SendAnswer(string roomId, object sdpAnswer)
        {
            if (!await IsParticipantAsync(roomId))
            {
                await Clients.Caller.SendAsync("Error", "You are not a participant of this room");
                return;
            }

            _logger.LogDebug("CallHub: relaying SDP answer in room {RoomId}", roomId);
            await Clients.OthersInGroup(roomId).SendAsync("ReceiveAnswer", sdpAnswer);
        }

        public async Task SendIceCandidate(string roomId, object candidate)
        {
            if (!await IsParticipantAsync(roomId))
            {
                await Clients.Caller.SendAsync("Error", "You are not a participant of this room");
                return;
            }

            _logger.LogDebug("CallHub: relaying ICE candidate in room {RoomId}", roomId);
            await Clients.OthersInGroup(roomId).SendAsync("ReceiveIceCandidate", candidate);
        }

        public async Task EndCall(string roomId)
        {
            if (!await IsParticipantAsync(roomId))
            {
                await Clients.Caller.SendAsync("Error", "You are not a participant of this room");
                return;
            }

            await _sessionService.EndSessionAsync(roomId, SessionEndReason.Completed);

            await Clients.Group(roomId).SendAsync("CallEnded", new
            {
                reason = SessionEndReason.Completed.ToString()
            });

            _logger.LogInformation("CallHub: call ended by user {UserId} in room {RoomId}", GetCurrentUserId(), roomId);
        }

        #endregion

        #region Private Helpers

        private Guid GetCurrentUserId()
        {
            var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        private async Task<bool> IsParticipantAsync(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId)) return false;

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return false;

            var session = await _sessionService.GetSessionByRoomIdAsync(roomId);
            return session != null && (session.DoctorId == userId || session.PatientId == userId);
        }

        #endregion
    }
}
