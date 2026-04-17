using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Appointment;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/doctors/me/dashboard")]
    [Authorize(Roles = "Doctor")]
    public class DoctorDashboardController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<DoctorDashboardController> _logger;

        public DoctorDashboardController(
            IDoctorService doctorService,
            IAppointmentService appointmentService,
            ILogger<DoctorDashboardController> logger)
        {
            _doctorService = doctorService;
            _appointmentService = appointmentService;
            _logger = logger;
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(ApiResponse<DoctorDashboardStatsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DoctorDashboardStatsResponse>>> GetDashboardStats()
        {
            var currentDoctorId = GetCurrentDoctorId();
            _logger.LogInformation("Get dashboard stats request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var stats = await _doctorService.GetDashboardStatsAsync(currentDoctorId);
                _logger.LogInformation("Dashboard stats retrieved successfully for doctor: {DoctorId}", currentDoctorId);
                
                return Ok(ApiResponse<DoctorDashboardStatsResponse>.Success(
                    stats,
                    "Dashboard statistics retrieved successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found for dashboard stats: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving dashboard statistics",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("appointments")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DoctorAppointmentResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DoctorAppointmentResponse>>>> GetMyAppointments(
            [FromQuery] GetDoctorAppointmentsRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();
            _logger.LogInformation(
                "Get appointments request for doctor: {DoctorId}. Page: {Page}, Size: {Size}, StartDate: {StartDate}, EndDate: {EndDate}, Status: {Status}, SortBy: {SortBy}, SortOrder: {SortOrder}",
                currentDoctorId, request.PageNumber, request.PageSize, request.StartDate, request.EndDate,
                request.Status, request.SortBy, request.SortOrder);

            try
            {
                var appointments = await _appointmentService.GetDoctorAppointmentsAsync(currentDoctorId, request);

                _logger.LogInformation(
                    "Appointments retrieved successfully for doctor: {DoctorId}. Count: {Count}, TotalCount: {TotalCount}",
                    currentDoctorId, appointments.Data.Count(), appointments.TotalCount);

                return Ok(ApiResponse<PaginatedResponse<DoctorAppointmentResponse>>.Success(
                    appointments,
                    "Appointments retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving appointments",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("appointments/today")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<TodayAppointmentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<TodayAppointmentResponse>>>> GetTodayAppointments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5)
        {
            var currentDoctorId = GetCurrentDoctorId();
            _logger.LogInformation("Get today's appointments request for doctor: {DoctorId}. Page: {Page}, Size: {Size}",
                currentDoctorId, pageNumber, pageSize);

            try
            {
                var paginationParams = new PaginationParams
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var appointments = await _doctorService.GetTodayAppointmentsAsync(currentDoctorId, paginationParams);
                _logger.LogInformation("Today's appointments retrieved successfully for doctor: {DoctorId}. Count: {Count}",
                    currentDoctorId, appointments.Data.Count());
                
                return Ok(ApiResponse<PaginatedResponse<TodayAppointmentResponse>>.Success(
                    appointments,
                    "Today's appointments retrieved successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found for today's appointments: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving today's appointments for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving today's appointments",
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
