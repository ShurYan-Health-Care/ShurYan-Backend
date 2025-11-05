using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Chat;
using Shuryan.Application.DTOs.Responses.Chat;
using Shuryan.Application.Interfaces;
using System.Security.Claims;

namespace Shuryan.API.Controllers
{
    /// <summary>
    /// Controller للتعامل مع الـ AI Chat Bot
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // لازم يكون User مسجل دخول
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        /// <summary>
        /// إرسال رسالة للـ AI Bot
        /// POST /api/chat/send-message
        /// </summary>
        [HttpPost("send-message")]
        [ProducesResponseType(typeof(ApiResponse<ChatMessageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ChatMessageResponse>>> SendMessage(
            [FromBody] SendMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("⚠️ Invalid send message request");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "بيانات غير صحيحة",
                    errors,
                    400
                ));
            }

            try
            {
                // جيب معلومات المستخدم من الـ Token
                var userId = GetUserId();
                var userRole = GetUserRole();

                if (userId == Guid.Empty || string.IsNullOrEmpty(userRole))
                {
                    _logger.LogWarning("⚠️ Unauthorized access attempt");
                    return Unauthorized(ApiResponse<object>.Failure(
                        "غير مصرح لك بالوصول",
                        null,
                        401
                    ));
                }

                _logger.LogInformation("💬 User {UserId} ({Role}) sending message", userId, userRole);

                var response = await _chatService.SendMessageAsync(userId, userRole, request);

                if (response == null)
                {
                    _logger.LogError("❌ Failed to process message");
                    return StatusCode(500, ApiResponse<object>.Failure(
                        "حدث خطأ أثناء معالجة الرسالة",
                        null,
                        500
                    ));
                }

                _logger.LogInformation("✅ Message processed successfully");

                return Ok(ApiResponse<ChatMessageResponse>.Success(
                    response,
                    "تم إرسال الرسالة بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error in SendMessage");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ غير متوقع",
                    null,
                    500
                ));
            }
        }

        /// <summary>
        /// جيب محادثة معينة مع كل رسائلها
        /// GET /api/chat/conversations/{conversationId}
        /// </summary>
        [HttpGet("conversations/{conversationId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ConversationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ConversationResponse>>> GetConversation(Guid conversationId)
        {
            try
            {
                var userId = GetUserId();

                var conversation = await _chatService.GetConversationAsync(conversationId, userId);

                if (conversation == null)
                {
                    return NotFound(ApiResponse<object>.Failure(
                        "المحادثة غير موجودة",
                        null,
                        404
                    ));
                }

                return Ok(ApiResponse<ConversationResponse>.Success(
                    conversation,
                    "تم جلب المحادثة بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in GetConversation");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ غير متوقع",
                    null,
                    500
                ));
            }
        }

        /// <summary>
        /// جيب كل محادثات المستخدم
        /// GET /api/chat/conversations
        /// </summary>
        [HttpGet("conversations")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConversationListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ConversationListItemResponse>>>> GetUserConversations(
            [FromQuery] bool activeOnly = true)
        {
            try
            {
                var userId = GetUserId();

                var conversations = await _chatService.GetUserConversationsAsync(userId, activeOnly);

                return Ok(ApiResponse<IEnumerable<ConversationListItemResponse>>.Success(
                    conversations,
                    "تم جلب المحادثات بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in GetUserConversations");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ غير متوقع",
                    null,
                    500
                ));
            }
        }

        /// <summary>
        /// امسح محادثة
        /// DELETE /api/chat/conversations/{conversationId}
        /// </summary>
        [HttpDelete("conversations/{conversationId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteConversation(Guid conversationId)
        {
            try
            {
                var userId = GetUserId();

                var success = await _chatService.DeleteConversationAsync(conversationId, userId);

                if (!success)
                {
                    return NotFound(ApiResponse<object>.Failure(
                        "المحادثة غير موجودة",
                        null,
                        404
                    ));
                }

                return Ok(ApiResponse<object>.Success(
                    null,
                    "تم حذف المحادثة بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in DeleteConversation");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ غير متوقع",
                    null,
                    500
                ));
            }
        }

        /// <summary>
        /// امسح كل رسائل محادثة
        /// DELETE /api/chat/conversations/{conversationId}/messages
        /// </summary>
        [HttpDelete("conversations/{conversationId:guid}/messages")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> ClearConversationMessages(Guid conversationId)
        {
            try
            {
                var userId = GetUserId();

                var success = await _chatService.ClearConversationMessagesAsync(conversationId, userId);

                if (!success)
                {
                    return NotFound(ApiResponse<object>.Failure(
                        "المحادثة غير موجودة",
                        null,
                        404
                    ));
                }

                return Ok(ApiResponse<object>.Success(
                    null,
                    "تم مسح الرسائل بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in ClearConversationMessages");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ غير متوقع",
                    null,
                    500
                ));
            }
        }

        #region Helper Methods

        /// <summary>
        /// جيب User ID من الـ Claims
        /// </summary>
        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>
        /// جيب User Role من الـ Claims
        /// </summary>
        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        #endregion
    }
}
