using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Appointment;
using Shuryan.Application.DTOs.Requests.Clinic;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.DTOs.Responses.Clinic;
using Shuryan.Application.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/Doctors/me")]
    [Authorize(Roles = "Doctor")]
    public class DoctorServicesController : ControllerBase
    {
        private readonly IDoctorServicePricingService _servicePricingService;
        private readonly IDoctorScheduleService _scheduleService;
        private readonly ILogger<DoctorServicesController> _logger;

        public DoctorServicesController(
            IDoctorServicePricingService servicePricingService,
            IDoctorScheduleService scheduleService,
            ILogger<DoctorServicesController> logger)
        {
            _servicePricingService = servicePricingService;
            _scheduleService = scheduleService;
            _logger = logger;
        }

        #region Helper Methods
        private Guid GetCurrentDoctorId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Guid.Empty;
            }
            return userId;
        }
        #endregion

        #region Services & Pricing
        [HttpGet("services/regular-checkup")]
        [ProducesResponseType(typeof(ApiResponse<ServicePricingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServicePricingResponse>>> GetRegularCheckup()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access regular checkup pricing - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get regular checkup pricing request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var pricing = await _servicePricingService.GetRegularCheckupAsync(currentDoctorId);
                if (pricing == null)
                {
                    _logger.LogWarning("Regular checkup pricing not found for doctor: {DoctorId}", currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Regular checkup pricing not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Regular checkup pricing retrieved successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<ServicePricingResponse>.Success(
                    pricing,
                    "Regular checkup pricing retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving regular checkup pricing for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving pricing",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("services/regular-checkup")]
        [ProducesResponseType(typeof(ApiResponse<ServicePricingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServicePricingResponse>>> UpdateRegularCheckup([FromBody] UpdateServicePricingRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update regular checkup pricing - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Update regular checkup pricing request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UpdateRegularCheckup for doctor: {DoctorId}. Errors: {Errors}",
                    currentDoctorId, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var pricing = await _servicePricingService.UpdateRegularCheckupAsync(currentDoctorId, request);
                _logger.LogInformation("Regular checkup pricing updated successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<ServicePricingResponse>.Success(
                    pricing,
                    "تم تحديث سعر الكشف العادي بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found for regular checkup pricing update: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating regular checkup pricing for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating pricing",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("services/re-examination")]
        [ProducesResponseType(typeof(ApiResponse<ServicePricingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServicePricingResponse>>> GetReExamination()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access re-examination pricing - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get re-examination pricing request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var pricing = await _servicePricingService.GetReExaminationAsync(currentDoctorId);
                if (pricing == null)
                {
                    _logger.LogWarning("Re-examination pricing not found for doctor: {DoctorId}", currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Re-examination pricing not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Re-examination pricing retrieved successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<ServicePricingResponse>.Success(
                    pricing,
                    "Re-examination pricing retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving re-examination pricing for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving pricing",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("services/re-examination")]
        [ProducesResponseType(typeof(ApiResponse<ServicePricingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ServicePricingResponse>>> UpdateReExamination([FromBody] UpdateServicePricingRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update re-examination pricing - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Update re-examination pricing request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UpdateReExamination for doctor: {DoctorId}. Errors: {Errors}",
                    currentDoctorId, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var pricing = await _servicePricingService.UpdateReExaminationAsync(currentDoctorId, request);
                _logger.LogInformation("Re-examination pricing updated successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<ServicePricingResponse>.Success(
                    pricing,
                    "تم تحديث سعر إعادة الكشف بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found for re-examination pricing update: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating re-examination pricing for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating pricing",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Appointment Settings - Weekly Schedule
        [HttpGet("appointments/schedule")]
        [ProducesResponseType(typeof(ApiResponse<WeeklyScheduleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<WeeklyScheduleResponse>>> GetWeeklySchedule()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access weekly schedule - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get weekly schedule request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var schedule = await _scheduleService.GetWeeklyScheduleAsync(currentDoctorId);
                _logger.LogInformation("Weekly schedule retrieved successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<WeeklyScheduleResponse>.Success(
                    schedule,
                    "Weekly schedule retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weekly schedule for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving schedule",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("appointments/schedule")]
        [ProducesResponseType(typeof(ApiResponse<WeeklyScheduleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<WeeklyScheduleResponse>>> UpdateWeeklySchedule([FromBody] UpdateWeeklyScheduleRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update weekly schedule - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Update weekly schedule request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UpdateWeeklySchedule for doctor: {DoctorId}. Errors: {Errors}",
                    currentDoctorId, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var schedule = await _scheduleService.UpdateWeeklyScheduleAsync(currentDoctorId, request);
                _logger.LogInformation("Weekly schedule updated successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<WeeklyScheduleResponse>.Success(
                    schedule,
                    "تم تحديث جدول المواعيد بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found for weekly schedule update: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating weekly schedule for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating schedule",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Appointment Settings - Exceptional Dates
        [HttpGet("appointments/exceptions")]
        [ProducesResponseType(typeof(ApiResponse<ExceptionalDatesListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ExceptionalDatesListResponse>>> GetExceptionalDates()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access exceptional dates - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get exceptional dates request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var dates = await _scheduleService.GetExceptionalDatesAsync(currentDoctorId);
                _logger.LogInformation("Exceptional dates retrieved successfully for doctor: {DoctorId}. Count: {Count}",
                    currentDoctorId, dates.ExceptionalDates.Count);
                return Ok(ApiResponse<ExceptionalDatesListResponse>.Success(
                    dates,
                    "Exceptional dates retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exceptional dates for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving exceptional dates",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("appointments/exceptions")]
        [ProducesResponseType(typeof(ApiResponse<ExceptionalDateResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ExceptionalDateResponse>>> AddExceptionalDate([FromBody] AddExceptionalDateRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to add exceptional date - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Add exceptional date request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for AddExceptionalDate for doctor: {DoctorId}. Errors: {Errors}",
                    currentDoctorId, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var date = await _scheduleService.AddExceptionalDateAsync(currentDoctorId, request);
                _logger.LogInformation("Exceptional date added successfully for doctor: {DoctorId}. Date ID: {DateId}",
                    currentDoctorId, date.Id);
                return CreatedAtAction(
                    nameof(GetExceptionalDates),
                    new { },
                    ApiResponse<ExceptionalDateResponse>.Success(
                        date,
                        "تم إضافة الموعد الاستثنائي بنجاح"
                    )
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for exceptional date addition: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for exceptional date addition: {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding exceptional date for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while adding exceptional date",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpDelete("appointments/exceptions/{exceptionId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> RemoveExceptionalDate(Guid exceptionId)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to remove exceptional date - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Remove exceptional date request for doctor: {DoctorId}. Exception ID: {ExceptionId}",
                currentDoctorId, exceptionId);

            try
            {
                var result = await _scheduleService.RemoveExceptionalDateAsync(currentDoctorId, exceptionId);
                if (!result)
                {
                    _logger.LogWarning("Failed to remove exceptional date {ExceptionId} for doctor: {DoctorId}",
                        exceptionId, currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Exceptional date not found or could not be deleted",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Exceptional date removed successfully for doctor: {DoctorId}. Exception ID: {ExceptionId}",
                    currentDoctorId, exceptionId);
                return Ok(ApiResponse<object>.Success(
                    null,
                    "تم حذف الموعد الاستثنائي بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for exceptional date removal: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for exceptional date removal: {DoctorId}", currentDoctorId);
                return StatusCode(403, ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 403
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing exceptional date for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while removing exceptional date",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion
    }
}
