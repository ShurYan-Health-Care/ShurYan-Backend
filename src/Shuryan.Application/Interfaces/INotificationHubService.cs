namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Service للتعامل مع SignalR Hub
    /// </summary>
    public interface INotificationHubService
    {
        Task SendNotificationToUserAsync(Guid userId, string title, string message, object? data = null);
    }
}
