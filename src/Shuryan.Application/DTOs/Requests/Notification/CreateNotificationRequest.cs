using Shuryan.Core.Enums.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Notification
{
    public class CreateNotificationRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Notification type is required")]
        public NotificationType Type { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Title must be between 3-500 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Message must be between 5-2000 characters")]
        public string Message { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Related entity type cannot exceed 100 characters")]
        public string? RelatedEntityType { get; set; }

        public Guid? RelatedEntityId { get; set; }

        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        public NotificationDeliveryMethod DeliveryMethod { get; set; } = NotificationDeliveryMethod.InApp;
    }
}

