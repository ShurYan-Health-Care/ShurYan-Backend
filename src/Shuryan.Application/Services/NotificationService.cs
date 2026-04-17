using Microsoft.Extensions.Logging;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.System;
using Shuryan.Core.Enums.Notifications;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationHubService _hubService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            INotificationHubService hubService,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _hubService = hubService;
            _logger = logger;
        }

        public async Task SendNotificationAsync(
            Guid userId,
            NotificationType type,
            string title,
            string message,
            Guid? relatedEntityId = null,
            string? relatedEntityType = null,
            NotificationPriority priority = NotificationPriority.Normal)
        {
            try
            {
                // 1️⃣ حفظ الإشعار في Database
                var notification = new Notification
                {
                    UserId = userId,
                    Type = type,
                    Title = title,
                    Message = message,
                    RelatedEntityId = relatedEntityId,
                    RelatedEntityType = relatedEntityType,
                    Priority = priority,
                    DeliveryMethod = NotificationDeliveryMethod.InApp,
                    IsSent = false,
                    IsRead = false
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Notification saved to DB for User {UserId}, Type: {Type}, Title: {Title}",
                    userId, type, title);

                // 2️⃣ إرسال الإشعار real-time عبر SignalR
                await SendRealtimeNotificationAsync(userId, title, message, new
                {
                    notificationId = notification.Id,
                    type = type.ToString(),
                    priority = priority.ToString(),
                    relatedEntityId,
                    relatedEntityType,
                    createdAt = notification.CreatedAt
                });

                // 3️⃣ تحديث حالة الإرسال
                notification.IsSent = true;
                notification.SentAt = DateTime.UtcNow;
                _unitOfWork.Notifications.Update(notification);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error sending notification to User {UserId}, Type: {Type}", 
                    userId, type);
                throw;
            }
        }

        public async Task SendRealtimeNotificationAsync(
            Guid userId,
            string title,
            string message,
            object? data = null)
        {
            try
            {
                await _hubService.SendNotificationToUserAsync(userId, title, message, data);

                _logger.LogInformation(
                    "Real-time notification sent to User {UserId} via SignalR", 
                    userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error sending real-time notification to User {UserId}", 
                    userId);
                // لا نرمي Exception هنا لأن فشل SignalR ما يمنعش باقي العملية
            }
        }
    }
}
