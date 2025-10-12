using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Core.Enums.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Notification
{
    public class NotificationResponse : BaseAuditableDto
    {
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? RelatedEntityType { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public NotificationPriority Priority { get; set; }
        public NotificationDeliveryMethod DeliveryMethod { get; set; }
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
        public string? FailureReason { get; set; }
    }
}

