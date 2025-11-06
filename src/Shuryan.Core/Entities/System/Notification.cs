using System;
using System.ComponentModel.DataAnnotations.Schema;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Notifications;

namespace Shuryan.Core.Entities.System
{
	public class Notification : AuditableEntity
	{
		[ForeignKey("User")]
		public Guid UserId { get; set; }

		public NotificationType Type { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
		public string? RelatedEntityType { get; set; }
		public Guid? RelatedEntityId { get; set; }
		public bool IsRead { get; set; } = false;
		public DateTime? ReadAt { get; set; }
		public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
		public NotificationDeliveryMethod DeliveryMethod { get; set; } = NotificationDeliveryMethod.InApp;
		public bool IsSent { get; set; } = false;
		public DateTime? SentAt { get; set; }
		public string? FailureReason { get; set; }

		// Navigation Property
		public virtual User User { get; set; } = null!;
	}
}