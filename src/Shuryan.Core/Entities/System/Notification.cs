using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Notifications;

namespace Shuryan.Core.Entities.System
{
	public class Notification
	{
		public Guid Id { get; set; }

		[ForeignKey("User")]
		public Guid UserId { get; set; } // اليوزر اللي هيتبعتله الاشعار

		public NotificationType Type { get; set; } // نوع الإشعار

		[Required, MaxLength(100)]
		public string Title { get; set; } = string.Empty; // عنوان الإشعار

		[Required, MaxLength(500)]
		public string Message { get; set; } = string.Empty; // محتوى الإشعار

		// ربط الإشعار بالكيان المرتبط 
		[MaxLength(50)]
		public string? RelatedEntityType { get; set; } // مثال: "Appointment", "LabOrder", "Prescription"

		public Guid? RelatedEntityId { get; set; } // الـ ID الخاص بالموعد/الطلب/الروشته

		// Reading status
		public bool IsRead { get; set; } = false;
		public DateTime? ReadAt { get; set; }

		public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

		// طريقة الإرسال
		public NotificationDeliveryMethod DeliveryMethod { get; set; } = NotificationDeliveryMethod.InApp;

		// حالة الإرسال
		public bool IsSent { get; set; } = false;
		public DateTime? SentAt { get; set; }

		[MaxLength(500)]
		public string? FailureReason { get; set; } // Reason for transmission failure (if any)

		public DateTime CreatedAt { get; set; }

		// Navigation Property
		public virtual User User { get; set; } = null!;
	}
}