using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Responses.Notification;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Enums.Notifications;
using Shuryan.Core.Interfaces.UnitOfWork;
using System.Security.Claims;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// جلب الإشعارات الغير مقروءة للمستخدم الحالي
        /// </summary>
        [HttpGet("unread")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<NotificationResponse>>>> GetUnreadNotifications()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
                }

                var notifications = await _unitOfWork.Notifications.GetUnreadNotificationsAsync(userId);

                var response = notifications.Select(n => new NotificationResponse
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Type = n.Type,
                    Title = n.Title,
                    Message = n.Message,
                    RelatedEntityType = n.RelatedEntityType,
                    RelatedEntityId = n.RelatedEntityId,
                    IsRead = n.IsRead,
                    ReadAt = n.ReadAt,
                    Priority = n.Priority,
                    DeliveryMethod = n.DeliveryMethod,
                    IsSent = n.IsSent,
                    SentAt = n.SentAt,
                    FailureReason = n.FailureReason,
                    CreatedAt = n.CreatedAt,
                    UpdatedAt = n.UpdatedAt
                });

                _logger.LogInformation("Retrieved {Count} unread notifications for User {UserId}", 
                    notifications.Count(), userId);

                return Ok(ApiResponse<IEnumerable<NotificationResponse>>.Success(
                    response, 
                    "Unread notifications retrieved successfully", 
                    200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread notifications");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving notifications", 
                    statusCode: 500));
            }
        }

        /// <summary>
        /// جلب كل الإشعارات للمستخدم الحالي (مع Pagination)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<NotificationResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<NotificationResponse>>>> GetAllNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var allNotifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
                var totalCount = allNotifications.Count();

                var notifications = allNotifications
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(n => new NotificationResponse
                    {
                        Id = n.Id,
                        UserId = n.UserId,
                        Type = n.Type,
                        Title = n.Title,
                        Message = n.Message,
                        RelatedEntityType = n.RelatedEntityType,
                        RelatedEntityId = n.RelatedEntityId,
                        IsRead = n.IsRead,
                        ReadAt = n.ReadAt,
                        Priority = n.Priority,
                        DeliveryMethod = n.DeliveryMethod,
                        IsSent = n.IsSent,
                        SentAt = n.SentAt,
                        FailureReason = n.FailureReason,
                        CreatedAt = n.CreatedAt,
                        UpdatedAt = n.UpdatedAt
                    })
                    .ToList();

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                var paginatedResponse = new PaginatedResponse<NotificationResponse>
                {
                    Data = notifications,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalCount = totalCount,
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < totalPages
                };

                _logger.LogInformation("Retrieved page {PageNumber} of notifications for User {UserId}", 
                    pageNumber, userId);

                return Ok(ApiResponse<PaginatedResponse<NotificationResponse>>.Success(
                    paginatedResponse, 
                    "Notifications retrieved successfully", 
                    200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving notifications", 
                    statusCode: 500));
            }
        }

        /// <summary>
        /// عدد الإشعارات الغير مقروءة
        /// </summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResponse<UnreadCountResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<UnreadCountResponse>>> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
                }

                var count = await _unitOfWork.Notifications.GetUnreadCountAsync(userId);

                var response = new UnreadCountResponse { Count = count };

                return Ok(ApiResponse<UnreadCountResponse>.Success(
                    response, 
                    "Unread count retrieved successfully", 
                    200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread count");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving unread count", 
                    statusCode: 500));
            }
        }

        /// <summary>
        /// تحديد إشعار معين كـ "مقروء"
        /// </summary>
        [HttpPut("{notificationId}/mark-as-read")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(Guid notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
                }

                var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
                
                if (notification == null)
                {
                    return NotFound(ApiResponse<object>.Failure(
                        "Notification not found", 
                        statusCode: 404));
                }

                if (notification.UserId != userId)
                {
                    return StatusCode(403, ApiResponse<object>.Failure(
                        "You don't have permission to access this notification", 
                        statusCode: 403));
                }

                await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);

                _logger.LogInformation("Notification {NotificationId} marked as read by User {UserId}", 
                    notificationId, userId);

                return Ok(ApiResponse<object>.Success(
                    null, 
                    "Notification marked as read", 
                    200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while marking notification as read", 
                    statusCode: 500));
            }
        }

        /// <summary>
        /// تحديد كل الإشعارات كـ "مقروءة"
        /// </summary>
        [HttpPut("mark-all-as-read")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
                }

                await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);

                _logger.LogInformation("All notifications marked as read for User {UserId}", userId);

                return Ok(ApiResponse<object>.Success(
                    null, 
                    "All notifications marked as read", 
                    200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while marking notifications as read", 
                    statusCode: 500));
            }
        }

        /// <summary>
        /// حذف إشعار معين
        /// </summary>
        [HttpDelete("{notificationId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteNotification(Guid notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
                }

                var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
                
                if (notification == null)
                {
                    return NotFound(ApiResponse<object>.Failure(
                        "Notification not found", 
                        statusCode: 404));
                }

                if (notification.UserId != userId)
                {
                    return StatusCode(403, ApiResponse<object>.Failure(
                        "You don't have permission to delete this notification", 
                        statusCode: 403));
                }

                _unitOfWork.Notifications.Delete(notification);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Notification {NotificationId} deleted by User {UserId}", 
                    notificationId, userId);

                return Ok(ApiResponse<object>.Success(
                    null, 
                    "Notification deleted successfully", 
                    200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while deleting notification", 
                    statusCode: 500));
            }
        }

#if DEBUG
        /// <summary>
        /// [DEBUG ONLY] Test endpoint لإرسال إشعار تجريبي - متاح فقط في Development
        /// </summary>
        [HttpPost("debug/test-send")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> TestSendNotification(
            [FromQuery] Guid? targetUserId = null)
        {
            try
            {
                var userId = targetUserId ?? GetCurrentUserId();
                
                if (userId == Guid.Empty)
                {
                    return BadRequest(ApiResponse<object>.Failure("Invalid user ID", statusCode: 400));
                }

                _logger.LogInformation("=== DEBUG: Sending test notification to User {UserId} ===", userId);

                await _notificationService.SendNotificationAsync(
                    userId: userId,
                    type: NotificationType.AppointmentCompleted,
                    title: "Test Notification",
                    message: "This is a test notification from DEBUG endpoint",
                    relatedEntityId: Guid.NewGuid(),
                    relatedEntityType: "Test",
                    priority: NotificationPriority.Normal
                );

                _logger.LogInformation("=== DEBUG: Test notification sent successfully ===");

                return Ok(ApiResponse<object>.Success(
                    new { userId, message = "Test notification sent" },
                    "Test notification sent successfully",
                    200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== DEBUG: Error sending test notification ===");
                return StatusCode(500, ApiResponse<object>.Failure(
                    $"Error: {ex.Message}",
                    statusCode: 500));
            }
        }
#endif

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }

    // Helper DTO
    public class UnreadCountResponse
    {
        public int Count { get; set; }
    }
}
