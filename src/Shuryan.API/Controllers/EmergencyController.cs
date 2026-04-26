using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Emergency;
using Shuryan.Application.DTOs.Responses.Emergency;
using Shuryan.Application.Interfaces;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmergencyController : ControllerBase
    {
        private readonly IEmergencyDispatchService _dispatchService;
        private readonly IEmergencyModeService _emergencyModeService;
        private readonly ILogger<EmergencyController> _logger;

        public EmergencyController(
            IEmergencyDispatchService dispatchService,
            IEmergencyModeService emergencyModeService,
            ILogger<EmergencyController> logger)
        {
            _dispatchService = dispatchService;
            _emergencyModeService = emergencyModeService;
            _logger = logger;
        }

        [HttpPost("dispatch")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DispatchSos([FromBody] SosDispatchRequest request)
        {
            var patientId = GetCurrentUserId();
            if (patientId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));

            _logger.LogInformation("SOS dispatch request received from patient {PatientId}", patientId);

            try
            {
                await _dispatchService.DispatchSosAsync(patientId, request);
                return Ok(ApiResponse<object>.Success(null, "تم إرسال نداء الطوارئ بنجاح"));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during SOS dispatch for patient {PatientId}", patientId);
                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء إرسال نداء الطوارئ", statusCode: 500));
            }
        }

        [HttpGet("doctor/patients")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmergencyPatientResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmergencyPatientResponse>>>> GetEmergencyPatients()
        {
            var doctorId = GetCurrentUserId();
            if (doctorId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));

            try
            {
                var patients = await _emergencyModeService.GetDoctorEmergencyPatientsAsync(doctorId);
                return Ok(ApiResponse<IEnumerable<EmergencyPatientResponse>>.Success(patients));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching emergency patients for doctor {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء جلب مرضى الطوارئ", statusCode: 500));
            }
        }

        [HttpGet("doctor/events")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmergencyEventResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmergencyEventResponse>>>> GetSosEvents()
        {
            var doctorId = GetCurrentUserId();
            if (doctorId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));

            try
            {
                var events = await _emergencyModeService.GetDoctorSosEventsAsync(doctorId);
                return Ok(ApiResponse<IEnumerable<EmergencyEventResponse>>.Success(events));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching SOS events for doctor {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء جلب سجل الطوارئ", statusCode: 500));
            }
        }

        [HttpPut("events/{eventId}/resolve")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> ResolveEvent([FromRoute] Guid eventId)
        {
            var doctorId = GetCurrentUserId();
            if (doctorId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));

            try
            {
                await _emergencyModeService.ResolveEmergencyEventAsync(doctorId, eventId);
                return Ok(ApiResponse<object>.Success(null, "تم حل حالة الطوارئ بنجاح"));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.Failure(ex.Message, statusCode: 401));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving emergency event {EventId}", eventId);
                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء حل حالة الطوارئ", statusCode: 500));
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
