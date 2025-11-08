using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Chat;
using Shuryan.Application.DTOs.Responses.Chat;
using Shuryan.Application.Interfaces;
using System.Security.Claims;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpPost("send-message")]
        [ProducesResponseType(typeof(ApiResponse<ChatMessageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ChatMessageResponse>>> SendMessage([FromBody] SendMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid send message request");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid data",
                    errors,
                    400
                ));
            }

            try
            {
                var userId = GetUserId();
                var userRole = GetUserRole();

                if (userId == Guid.Empty || string.IsNullOrEmpty(userRole))
                {
                    _logger.LogWarning("Unauthorized access attempt");
                    return Unauthorized(ApiResponse<object>.Failure(
                        "Unauthorized access",
                        null,
                        401
                    ));
                }

                _logger.LogInformation("User {UserId} ({Role}) sending message", userId, userRole);

                var response = await _chatService.SendMessageAsync(userId, userRole, request);

                if (response == null)
                {
                    _logger.LogError("Failed to process message");
                    return StatusCode(500, ApiResponse<object>.Failure(
                        "An error occurred while processing the message",
                        null,
                        500
                    ));
                }

                _logger.LogInformation("Message processed successfully");

                return Ok(ApiResponse<ChatMessageResponse>.Success(
                    response,
                    "Message sent successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in SendMessage");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred",
                    null,
                    500
                ));
            }
        }

        [HttpGet("history")]
        [ProducesResponseType(typeof(ApiResponse<ChatHistoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ChatHistoryResponse>>> GetChatHistory(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                if (pageNumber < 1)
                {
                    return BadRequest(ApiResponse<object>.Failure(
                        "Page number must be greater than or equal to 1",
                        null,
                        400
                    ));
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(ApiResponse<object>.Failure(
                        "Page size must be between 1 and 100",
                        null,
                        400
                    ));
                }

                var userId = GetUserId();
                var history = await _chatService.GetChatHistoryAsync(userId, pageNumber, pageSize);

                if (history == null)
                {
                    return NotFound(ApiResponse<object>.Failure(
                        "No conversation found",
                        null,
                        404
                    ));
                }

                return Ok(ApiResponse<ChatHistoryResponse>.Success(
                    history,
                    "Chat history retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetChatHistory");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred",
                    null,
                    500
                ));
            }
        }

        [HttpDelete("clear")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> ClearChat()
        {
            try
            {
                var userId = GetUserId();
                var success = await _chatService.ClearUserChatAsync(userId);

                if (!success)
                {
                    return NotFound(ApiResponse<object>.Failure(
                        "No conversation to clear",
                        null,
                        404
                    ));
                }

                return Ok(ApiResponse<object>.Success(
                    null,
                    "Chat cleared successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ClearChat");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred",
                    null,
                    500
                ));
            }
        }

        #region Helper Methods
        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
        #endregion
    }
}
