using Shuryan.Core.Enums.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Notification
{
    public class CreateNotificationRequest
    {
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? RelatedEntityType { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public NotificationDeliveryMethod DeliveryMethod { get; set; } = NotificationDeliveryMethod.InApp;
    }
}

