using System;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    public interface IVideoNotificationHubService
    {
        Task NotifySessionReadyAsync(Guid patientId, Guid appointmentId, string channelName);
        Task NotifySessionEndedAsync(Guid otherParticipantId, Guid appointmentId);
    }
}