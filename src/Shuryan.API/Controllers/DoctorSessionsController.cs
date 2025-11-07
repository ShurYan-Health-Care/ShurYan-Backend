using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Responses.Session;
using Shuryan.Application.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/doctors/me/sessions")]
    [Authorize(Roles = "Doctor")]
    public class DoctorSessionsController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly ILogger<DoctorSessionsController> _logger;

        public DoctorSessionsController(
            ISessionService sessionService,
            ILogger<DoctorSessionsController> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<SessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<SessionResponse>>> GetMyActiveSession()
        {
            var currentDoctorId = GetCurrentDoctorId();

            try
            {
                _logger.LogInformation("Getting active session for Doctor {DoctorId}", currentDoctorId);

                var activeSession = await _sessionService.GetDoctorCurrentActiveSessionAsync(currentDoctorId);

                if (activeSession == null)
                {
                    _logger.LogInformation("No active session found for Doctor {DoctorId}", currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        "لا توجد جلسة نشطة حالياً",
                        new[] { "لا توجد جلسة نشطة حالياً" },
                        404
                    ));
                }

                _logger.LogInformation("Active session found for Doctor {DoctorId} with Appointment {AppointmentId}",
                    currentDoctorId, activeSession.AppointmentId);

                return Ok(ApiResponse<SessionResponse>.Success(
                    activeSession,
                    "تم جلب الجلسة النشطة بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active session for Doctor {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ غير متوقع",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #region Helper Methods

        private Guid GetCurrentDoctorId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        #endregion
    }
}
