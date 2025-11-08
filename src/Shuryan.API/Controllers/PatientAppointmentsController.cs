using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Appointment;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/patients/me/appointments")]
    [Authorize(Roles = "Patient")]
    public class PatientAppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<PatientAppointmentsController> _logger;

        public PatientAppointmentsController(
            IAppointmentService appointmentService,
            ILogger<PatientAppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;
        }

        #region GET Operations

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AppointmentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResponse>>>> GetMyAppointments()
        {
            var currentPatientId = GetCurrentPatientId();
            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access appointments");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get my appointments request for patient: {PatientId}", currentPatientId);

            try
            {
                var appointments = await _appointmentService.GetAppointmentsByPatientIdAsync(currentPatientId);
                _logger.LogInformation("Retrieved {Count} appointments for patient: {PatientId}", appointments.Count(), currentPatientId);
                return Ok(ApiResponse<IEnumerable<AppointmentResponse>>.Success(
                    appointments,
                    $"Retrieved {appointments.Count()} appointments successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for patient: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving appointments",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{appointmentId}")]
        [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AppointmentResponse>> GetAppointmentById(Guid appointmentId)
        {
            _logger.LogInformation("Get appointment by ID request: {AppointmentId}", appointmentId);

            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment not found: {AppointmentId}", appointmentId);
                    return NotFound(new { Message = $"Appointment with ID {appointmentId} not found" });
                }

                var currentPatientId = GetCurrentPatientId();
                if (!IsAccessingOwnData(appointment.PatientId))
                {
                    _logger.LogWarning("Patient {CurrentPatientId} attempted to access appointment {AppointmentId} for patient {PatientId}", currentPatientId, appointmentId, appointment.PatientId);
                    return Forbid();
                }

                _logger.LogInformation("Appointment retrieved successfully: {AppointmentId}", appointmentId);
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment: {AppointmentId}", appointmentId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving appointment" });
            }
        }

        [HttpGet("upcoming")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetMyUpcomingAppointments()
        {
            var currentPatientId = GetCurrentPatientId();
            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access upcoming appointments");
                return Unauthorized(new { Message = "Invalid or missing authentication token" });
            }

            _logger.LogInformation("Get upcoming appointments request for patient: {PatientId}", currentPatientId);

            try
            {
                var appointments = await _appointmentService.GetUpcomingAppointmentsAsync(currentPatientId, "Patient");
                _logger.LogInformation("Retrieved {Count} upcoming appointments for patient: {PatientId}", appointments.Count(), currentPatientId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming appointments for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving upcoming appointments" });
            }
        }

        [HttpGet("past")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetMyPastAppointments()
        {
            var currentPatientId = GetCurrentPatientId();
            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access past appointments");
                return Unauthorized(new { Message = "Invalid or missing authentication token" });
            }

            _logger.LogInformation("Get past appointments request for patient: {PatientId}", currentPatientId);

            try
            {
                var appointments = await _appointmentService.GetPastAppointmentsAsync(currentPatientId, "Patient");
                _logger.LogInformation("Retrieved {Count} past appointments for patient: {PatientId}", appointments.Count(), currentPatientId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving past appointments for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving past appointments" });
            }
        }

        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetMyAppointmentsByStatus(AppointmentStatus status)
        {
            var currentPatientId = GetCurrentPatientId();
            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access appointments by status");
                return Unauthorized(new { Message = "Invalid or missing authentication token" });
            }

            _logger.LogInformation("Get appointments by status request for patient: {PatientId}, Status: {Status}", currentPatientId, status);

            try
            {
                var allAppointments = await _appointmentService.GetAppointmentsByStatusAsync(status);
                var myAppointments = allAppointments.Where(a => a.PatientId == currentPatientId);
                _logger.LogInformation("Retrieved {Count} appointments with status {Status} for patient: {PatientId}", myAppointments.Count(), status, currentPatientId);
                return Ok(myAppointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments by status for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving appointments" });
            }
        }

        [HttpGet("date-range")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetMyAppointmentsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var currentPatientId = GetCurrentPatientId();
            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access appointments by date range");
                return Unauthorized(new { Message = "Invalid or missing authentication token" });
            }

            _logger.LogInformation("Get appointments by date range request for patient: {PatientId}, Range: {StartDate} - {EndDate}", currentPatientId, startDate, endDate);

            try
            {
                var allAppointments = await _appointmentService.GetAppointmentsByDateRangeAsync(startDate, endDate);
                var myAppointments = allAppointments.Where(a => a.PatientId == currentPatientId);
                _logger.LogInformation("Retrieved {Count} appointments in date range for patient: {PatientId}", myAppointments.Count(), currentPatientId);
                return Ok(myAppointments);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid date range for appointments: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments by date range for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An error occurred while retrieving appointments" });
            }
        }

        [HttpGet("count")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetMyAppointmentsCount()
        {
            var currentPatientId = GetCurrentPatientId();
            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access appointments count");
                return Unauthorized(new { Message = "Invalid or missing authentication token" });
            }

            _logger.LogInformation("Get appointments count request for patient: {PatientId}", currentPatientId);

            try
            {
                var count = await _appointmentService.GetAppointmentsCountAsync(currentPatientId, "Patient");
                _logger.LogInformation("Appointments count: {Count} for patient: {PatientId}", count, currentPatientId);
                return Ok(new { PatientId = currentPatientId, AppointmentsCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments count for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving count" });
            }
        }

        [HttpGet("check-availability")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CheckTimeSlotAvailability(
            [FromQuery] Guid doctorId,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime)
        {
            if (doctorId == Guid.Empty)
            {
                _logger.LogWarning("Check availability request with empty doctor ID");
                return BadRequest(new { Message = "Doctor ID is required" });
            }

            _logger.LogInformation("Check time slot availability for doctor: {DoctorId}, Time: {StartTime} - {EndTime}", doctorId, startTime, endTime);

            try
            {
                var isAvailable = await _appointmentService.IsTimeSlotAvailableAsync(doctorId, startTime, endTime);
                _logger.LogInformation("Time slot availability for doctor {DoctorId}: {IsAvailable}", doctorId, isAvailable);
                return Ok(new
                {
                    DoctorId = doctorId,
                    StartTime = startTime,
                    EndTime = endTime,
                    IsAvailable = isAvailable
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for checking availability: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for doctor: {DoctorId}", doctorId);
                return StatusCode(500, new { Message = "An error occurred while checking availability" });
            }
        }

        #endregion

        #region POST/PUT/PATCH/DELETE Operations

        [HttpPost]
        [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AppointmentResponse>> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid create appointment request");
                return BadRequest(ModelState);
            }

            var currentPatientId = GetCurrentPatientId();
            if (!IsAccessingOwnData(request.PatientId))
            {
                _logger.LogWarning("Patient {CurrentPatientId} attempted to create appointment for patient {RequestedPatientId}", currentPatientId, request.PatientId);
                return Forbid();
            }

            _logger.LogInformation("Create appointment request for patient: {PatientId}", request.PatientId);

            try
            {
                var appointment = await _appointmentService.CreateAppointmentAsync(request);
                _logger.LogInformation("Appointment created successfully: {AppointmentId} for patient: {PatientId}", appointment.Id, request.PatientId);
                return CreatedAtAction(
                    nameof(GetAppointmentById),
                    new { appointmentId = appointment.Id },
                    appointment);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for creating appointment: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for creating appointment: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment for patient: {PatientId}", request.PatientId);
                return StatusCode(500, new { Message = "An error occurred while creating the appointment" });
            }
        }

        [HttpPut("{appointmentId}")]
        [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppointmentResponse>> UpdateAppointment(
            Guid appointmentId,
            [FromBody] UpdateAppointmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update appointment request for appointment: {AppointmentId}", appointmentId);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Update appointment request: {AppointmentId}", appointmentId);

            var existingAppointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            if (existingAppointment == null)
            {
                _logger.LogWarning("Appointment not found for update: {AppointmentId}", appointmentId);
                return NotFound(new { Message = $"Appointment with ID {appointmentId} not found" });
            }

            var currentPatientId = GetCurrentPatientId();
            if (!IsAccessingOwnData(existingAppointment.PatientId))
            {
                _logger.LogWarning("Patient {CurrentPatientId} attempted to update appointment {AppointmentId} for patient {PatientId}", currentPatientId, appointmentId, existingAppointment.PatientId);
                return Forbid();
            }

            try
            {
                var appointment = await _appointmentService.UpdateAppointmentAsync(appointmentId, request);
                _logger.LogInformation("Appointment updated successfully: {AppointmentId}", appointmentId);
                return Ok(appointment);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for updating appointment: {AppointmentId}", appointmentId);
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for updating appointment: {AppointmentId}", appointmentId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment: {AppointmentId}", appointmentId);
                return StatusCode(500, new { Message = "An error occurred while updating the appointment" });
            }
        }

        [HttpPatch("{appointmentId}/cancel")]
        [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppointmentResponse>> CancelAppointment(
            Guid appointmentId,
            [FromBody] CancelAppointmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid cancel appointment request for appointment: {AppointmentId}", appointmentId);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Cancel appointment request: {AppointmentId}", appointmentId);

            var existingAppointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            if (existingAppointment == null)
            {
                _logger.LogWarning("Appointment not found for cancellation: {AppointmentId}", appointmentId);
                return NotFound(new { Message = $"Appointment with ID {appointmentId} not found" });
            }

            var currentPatientId = GetCurrentPatientId();
            if (!IsAccessingOwnData(existingAppointment.PatientId))
            {
                _logger.LogWarning("Patient {CurrentPatientId} attempted to cancel appointment {AppointmentId} for patient {PatientId}", currentPatientId, appointmentId, existingAppointment.PatientId);
                return Forbid();
            }

            try
            {
                var appointment = await _appointmentService.CancelAppointmentAsync(appointmentId, request);
                _logger.LogInformation("Appointment cancelled successfully: {AppointmentId}", appointmentId);
                return Ok(appointment);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for cancelling appointment: {AppointmentId}", appointmentId);
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for cancelling appointment: {AppointmentId}", appointmentId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment: {AppointmentId}", appointmentId);
                return StatusCode(500, new { Message = "An error occurred while cancelling the appointment" });
            }
        }

        [HttpPatch("{appointmentId}/reschedule")]
        [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppointmentResponse>> RescheduleAppointment(
            Guid appointmentId,
            [FromBody] RescheduleAppointmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid reschedule appointment request for appointment: {AppointmentId}", appointmentId);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Reschedule appointment request: {AppointmentId}", appointmentId);

            var existingAppointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            if (existingAppointment == null)
            {
                _logger.LogWarning("Appointment not found for rescheduling: {AppointmentId}", appointmentId);
                return NotFound(new { Message = $"Appointment with ID {appointmentId} not found" });
            }

            var currentPatientId = GetCurrentPatientId();
            if (!IsAccessingOwnData(existingAppointment.PatientId))
            {
                _logger.LogWarning("Patient {CurrentPatientId} attempted to reschedule appointment {AppointmentId} for patient {PatientId}", currentPatientId, appointmentId, existingAppointment.PatientId);
                return Forbid();
            }

            try
            {
                var appointment = await _appointmentService.RescheduleAppointmentAsync(appointmentId, request);
                _logger.LogInformation("Appointment rescheduled successfully: {AppointmentId}", appointmentId);
                return Ok(appointment);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for rescheduling appointment: {AppointmentId}", appointmentId);
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for rescheduling appointment: {AppointmentId}", appointmentId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescheduling appointment: {AppointmentId}", appointmentId);
                return StatusCode(500, new { Message = "An error occurred while rescheduling the appointment" });
            }
        }

        [HttpDelete("{appointmentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteAppointment(Guid appointmentId)
        {
            _logger.LogInformation("Delete appointment request: {AppointmentId}", appointmentId);

            var existingAppointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            if (existingAppointment == null)
            {
                _logger.LogWarning("Appointment not found for deletion: {AppointmentId}", appointmentId);
                return NotFound(new { Message = $"Appointment with ID {appointmentId} not found" });
            }

            var currentPatientId = GetCurrentPatientId();
            if (!IsAccessingOwnData(existingAppointment.PatientId))
            {
                _logger.LogWarning("Patient {CurrentPatientId} attempted to delete appointment {AppointmentId} for patient {PatientId}", currentPatientId, appointmentId, existingAppointment.PatientId);
                return Forbid();
            }

            try
            {
                var result = await _appointmentService.DeleteAppointmentAsync(appointmentId);
                if (!result)
                {
                    _logger.LogWarning("Appointment not found for deletion: {AppointmentId}", appointmentId);
                    return NotFound(new { Message = $"Appointment with ID {appointmentId} not found" });
                }

                _logger.LogInformation("Appointment deleted successfully: {AppointmentId}", appointmentId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment: {AppointmentId}", appointmentId);
                return StatusCode(500, new { Message = "An error occurred while deleting the appointment" });
            }
        }

        #endregion

        #region Helper Methods

        private Guid GetCurrentPatientId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private bool IsAccessingOwnData(Guid patientId)
        {
            var currentUserId = GetCurrentPatientId();
            return currentUserId == patientId;
        }

        #endregion
    }
}
