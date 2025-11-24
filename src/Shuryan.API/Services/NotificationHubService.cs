using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Shuryan.API.Hubs;
using Shuryan.Application.Interfaces;

namespace Shuryan.API.Services
{
    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationHubService> _logger;

        public NotificationHubService(
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationHubService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendNotificationToUserAsync(Guid userId, string title, string message, object? data = null)
        {
            var groupName = $"user_{userId}";
            
            _logger.LogInformation(
                "Sending SignalR notification to group '{GroupName}' (User: {UserId})", 
                groupName, userId);

            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", new
            {
                title,
                message,
                timestamp = DateTime.UtcNow,
                data
            });
            
            _logger.LogInformation(
                "SignalR notification sent successfully to group '{GroupName}'", 
                groupName);
        }
    }
}
