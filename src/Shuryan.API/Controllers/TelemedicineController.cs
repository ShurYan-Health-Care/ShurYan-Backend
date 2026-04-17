using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Telemedicine;
using Shuryan.Application.DTOs.Responses.Telemedicine;
using Shuryan.Application.Interfaces;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/telemedicine")]
    [Authorize]
    public class TelemedicineController : ControllerBase
    {
        private readonly ICallSessionService _callSessionService;
        private readonly ILogger<TelemedicineController> _logger;

        public TelemedicineController(
            ICallSessionService callSessionService,
            ILogger<TelemedicineController> logger)
        {
            _callSessionService = callSessionService;
            _logger = logger;
        }

        #region Session Management

        /// <summary>
        /// Doctor creates a telemedicine session for a confirmed appointment.
        /// Returns the session including the RoomId needed to connect to the hub.
        /// </summary>
        [HttpPost("sessions")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<TelemedicineSessionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            var doctorId = GetCurrentUserId();
            if (doctorId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Failure("Invalid token", statusCode: 401));

            _logger.LogInformation("Doctor {DoctorId} creating session for appointment {AppointmentId}",
                doctorId, request.AppointmentId);

            var result = await _callSessionService.CreateSessionAsync(request.AppointmentId, doctorId);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    403 => StatusCode(StatusCodes.Status403Forbidden, result),
                    409 => Conflict(result),
                    _ => BadRequest(result)
                };
            }

            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Doctor or Patient retrieves an existing telemedicine session for an appointment.
        /// Used by the patient to obtain the RoomId before connecting to the hub.
        /// </summary>
        [HttpGet("sessions/appointment/{appointmentId:guid}")]
        [Authorize(Roles = "Doctor,Patient")]
        [ProducesResponseType(typeof(ApiResponse<TelemedicineSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSessionByAppointment([FromRoute] Guid appointmentId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Failure("Invalid token", statusCode: 401));

            _logger.LogInformation("User {UserId} fetching session for appointment {AppointmentId}",
                userId, appointmentId);

            var result = await _callSessionService.GetSessionByAppointmentIdAsync(appointmentId, userId);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    403 => StatusCode(StatusCodes.Status403Forbidden, result),
                    _ => BadRequest(result)
                };
            }

            return Ok(result);
        }

        #endregion

        #region ICE Configuration

        /// <summary>
        /// Returns STUN/TURN server configuration needed by the WebRTC client.
        /// Must be called before establishing the peer connection.
        /// </summary>
        [HttpGet("ice-config")]
        [Authorize(Roles = "Doctor,Patient")]
        [ProducesResponseType(typeof(ApiResponse<IceConfigResponse>), StatusCodes.Status200OK)]
        public IActionResult GetIceConfig()
        {
            var config = _callSessionService.GetIceConfig();
            return Ok(ApiResponse<IceConfigResponse>.Success(config, "ICE configuration retrieved"));
        }

        #endregion

        #region Private Helpers

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        #endregion
    }
}
