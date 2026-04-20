using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Shuryan.Application.Interfaces;
using Shuryan.API.Hubs;

namespace Shuryan.API.Services
{
    public class VideoNotificationHubService : IVideoNotificationHubService
    {
        private readonly IHubContext<VideoNotificationHub> _hubContext;

        public VideoNotificationHubService(IHubContext<VideoNotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifySessionReadyAsync(Guid patientId, Guid appointmentId, string channelName)
        {
            await _hubContext.Clients
                .Group(patientId.ToString())
                .SendAsync("SessionReady", new { appointmentId, channelName });
        }

        public async Task NotifySessionEndedAsync(Guid otherParticipantId, Guid appointmentId)
        {
            await _hubContext.Clients
                .Group(otherParticipantId.ToString())
                .SendAsync("SessionEnded", new { appointmentId });
        }
    }
}
