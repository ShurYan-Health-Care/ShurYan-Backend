using Shuryan.Core.Entities.System;
using Shuryan.Core.Enums.Notifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId);
        Task<IEnumerable<Notification>> GetByTypeAsync(Guid userId, NotificationType type);
        Task MarkAllAsReadAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId);
        Task DeleteOldNotificationsAsync(DateTime beforeDate);
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}

