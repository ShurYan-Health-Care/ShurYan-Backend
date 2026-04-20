using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Responses.VideoCall;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/video-sessions")]
    [Authorize]
    public class VideoCallController : ControllerBase
    {
        private readonly IVideoSessionService _videoSessionService;
        private readonly ILogger<VideoCallController> _logger;

        public VideoCallController(
            IVideoSessionService videoSessionService,
            ILogger<VideoCallController> logger)
        {
            _videoSessionService = videoSessionService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        [HttpGet("{appointmentId:guid}")]
        [Authorize(Roles = "Doctor,Patient")]
        [ProducesResponseType(typeof(ApiResponse<VideoSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSession([FromRoute] Guid appointmentId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Failure("Invalid token.", statusCode: 401));

            var result = await _videoSessionService.GetSessionAsync(appointmentId, userId);
            return StatusCode(result.StatusCode ?? 200, result);
        }

        [HttpPost("{appointmentId:guid}/join")]
        [Authorize(Roles = "Doctor,Patient")]
        [ProducesResponseType(typeof(ApiResponse<VideoSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> JoinSession([FromRoute] Guid appointmentId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Failure("Invalid token.", statusCode: 401));

            var result = await _videoSessionService.JoinSessionAsync(appointmentId, userId);
            return StatusCode(result.StatusCode ?? 200, result);
        }

        [HttpPost("{appointmentId:guid}/end")]
        [Authorize(Roles = "Doctor,Patient")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EndSession(
            [FromRoute] Guid appointmentId,
            [FromQuery] VideoSessionEndReason reason = VideoSessionEndReason.Completed)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Failure("Invalid token.", statusCode: 401));

            var result = await _videoSessionService.EndSessionAsync(appointmentId, userId, reason);
            return StatusCode(result.StatusCode ?? 200, result);
        }
    }
}
