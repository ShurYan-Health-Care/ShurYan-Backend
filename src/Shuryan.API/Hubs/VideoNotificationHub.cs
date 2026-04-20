using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Shuryan.API.Hubs
{
    [Authorize]
    public class VideoNotificationHub : Hub
    {
        private readonly ILogger<VideoNotificationHub> _logger;

        public VideoNotificationHub(ILogger<VideoNotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                _logger.LogInformation("VideoNotificationHub: User {UserId} connected ({ConnectionId})", userId, Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                _logger.LogInformation("VideoNotificationHub: User {UserId} disconnected ({ConnectionId})", userId, Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
