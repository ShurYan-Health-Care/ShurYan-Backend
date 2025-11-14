using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Appointment;
using Shuryan.Application.DTOs.Requests.Documentation;
using Shuryan.Application.DTOs.Requests.LabTests;
using Shuryan.Application.DTOs.Requests.Prescription;
using Shuryan.Application.DTOs.Requests.Session;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.DTOs.Responses.Documentation;
using Shuryan.Application.DTOs.Responses.LabTests;
using Shuryan.Application.DTOs.Responses.Prescription;
using Shuryan.Application.DTOs.Responses.Session;
using Shuryan.Application.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ISessionService _sessionService;
        private readonly IDocumentationService _documentationService;
        private readonly IPrescriptionService _prescriptionService;
        private readonly ILabTestService _labTestService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            IAppointmentService appointmentService,
            ISessionService sessionService,
            IDocumentationService documentationService,
            IPrescriptionService prescriptionService,
            ILabTestService labTestService,
            ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _sessionService = sessionService;
            _documentationService = documentationService;
            _prescriptionService = prescriptionService;
            _labTestService = labTestService;
            _logger = logger;
        }

        #region Helper Methods
        private Guid GetCurrentPatientId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Guid.Empty;
            }
            return userId;
        }

        private Guid GetCurrentDoctorId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Guid.Empty;
            }
            return userId;
        }

        private bool IsPatient()
        {
            return User.IsInRole("Patient");
        }

        private bool IsDoctor()
        {
            return User.IsInRole("Doctor");
        }
        #endregion

        #region Booking System
        [HttpPost("book")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(ApiResponse<BookedAppointmentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<BookedAppointmentResponse>>> BookAppointment([FromBody] BookAppointmentRequest request)
        {
            var patientId = GetCurrentPatientId();

            if (patientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized booking attempt - invalid token");
                return Unauthorized(ApiResponse<object>.Failure("Invalid or missing authentication token", statusCode: 401));
            }

            _logger.LogInformation("Booking appointment for Patient {PatientId} with Doctor {DoctorId} on {Date} at {Time}",
                patientId, request.DoctorId, request.AppointmentDate, request.AppointmentTime);

            try
            {
                var bookedAppointment = await _appointmentService.BookAppointmentAsync(patientId, request);

                _logger.LogInformation("Successfully booked appointment {AppointmentId} for Patient {PatientId}",
                    bookedAppointment.Id, patientId);

                return CreatedAtAction(
                    nameof(BookAppointment),
                    new { id = bookedAppointment.Id },
                    ApiResponse<BookedAppointmentResponse>.Success(bookedAppointment, "The appointment has been successfully booked", 201)
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while booking appointment for Patient {PatientId}", patientId);
                return BadRequest(ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 400));
            }
            catch (InvalidOperationException ex)
            {
                // Check if it's a "slot already booked" error
                if (ex.Message.Contains("booked"))
                {
                    _logger.LogWarning(ex, "Appointment slot conflict for Patient {PatientId}", patientId);
                    return Conflict(ApiResponse<object>.Failure("This date is already booked.", new[] { ex.Message }, 409));
                }

                _logger.LogWarning(ex, "Business logic error while booking appointment for Patient {PatientId}", patientId);
                return BadRequest(ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while booking appointment for Patient {PatientId}", patientId);
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred while booking your appointment.", new[] { ex.Message }, 500));
            }
        }
        #endregion

        #region Get Appointment Details
        [HttpGet("{appointmentId}")]
        [Authorize(Roles = "Doctor,Patient")]
        [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AppointmentResponse>>> GetAppointment(Guid appointmentId)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
                
                if (appointment == null)
                {
                    return NotFound(ApiResponse<object>.Failure(
                        "Appointment not found",
                        new[] { "Appointment not found" },
                        404
                    ));
                }

                // Checking permissions - only doctor or patient
                var userId = GetCurrentDoctorId();
                var isDoctor = IsDoctor();
                var isPatient = IsPatient();

                if (isDoctor && appointment.DoctorId != userId)
                {
                    return Unauthorized(ApiResponse<object>.Failure(
                        "Unauthorized access to this appointment",
                        new[] { "Unauthorized access" },
                        401
                    ));
                }

                if (isPatient && appointment.PatientId != userId)
                {
                    return Unauthorized(ApiResponse<object>.Failure(
                        "Unauthorized access to this appointment",
                        new[] { "Unauthorized access" },
                        401
                    ));
                }

                return Ok(ApiResponse<AppointmentResponse>.Success(
                    appointment,
                    "Appointment details retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving appointment details",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Session Management
        [HttpPost("{appointmentId}/start-session")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<SessionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<SessionResponse>>> StartSession(Guid appointmentId)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
            }

            try
            {
                var session = await _sessionService.StartSessionAsync(appointmentId, doctorId);
                return CreatedAtAction(
                    nameof(GetActiveSession),
                    new { appointmentId },
                    ApiResponse<SessionResponse>.Success(session, "Session started successfully", 201)
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 400));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 409));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting session for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred", new[] { ex.Message }, 500));
            }
        }

        [HttpGet("{appointmentId}/session")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<SessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<SessionResponse>>> GetActiveSession(Guid appointmentId)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
            }

            try
            {
                var session = await _sessionService.GetActiveSessionAsync(appointmentId, doctorId);
                if (session == null)
                {
                    return NotFound(ApiResponse<object>.Failure("No active session for this appointment", statusCode: 404));
                }

                return Ok(ApiResponse<SessionResponse>.Success(session, "Session retrieved successfully", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active session for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred", new[] { ex.Message }, 500));
            }
        }

        [HttpPost("{appointmentId}/end-session")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<EndSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<EndSessionResponse>>> EndSession(Guid appointmentId)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
            }

            try
            {
                var result = await _sessionService.EndSessionAsync(appointmentId, doctorId);
                return Ok(ApiResponse<EndSessionResponse>.Success(result, "Session ended successfully", 200));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 400));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 404));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred", new[] { ex.Message }, 500));
            }
        }
        #endregion

        #region Documentation
        [HttpPost("{appointmentId}/documentation")]
        [HttpPut("{appointmentId}/documentation")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<DocumentationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<DocumentationResponse>>> SaveDocumentation(Guid appointmentId, [FromBody] SaveDocumentationRequest request)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
            }

            try
            {
                var documentation = await _documentationService.SaveDocumentationAsync(appointmentId, doctorId, request);
                return Ok(ApiResponse<DocumentationResponse>.Success(documentation, "Documentation saved successfully", 200));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 400));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving documentation for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred", new[] { ex.Message }, 500));
            }
        }

        [HttpGet("{appointmentId}/documentation")]
        [Authorize(Roles = "Doctor,Patient")]
        [ProducesResponseType(typeof(ApiResponse<DocumentationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<DocumentationResponse>>> GetDocumentation(Guid appointmentId)
        {
            var userId = GetCurrentDoctorId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
            }

            var isDoctor = IsDoctor();

            try
            {
                var documentation = await _documentationService.GetDocumentationAsync(appointmentId, userId, isDoctor);
                if (documentation == null)
                {
                    return NotFound(ApiResponse<object>.Failure("No documentation for this appointment", statusCode: 404));
                }

                return Ok(ApiResponse<DocumentationResponse>.Success(documentation, "Documentation retrieved successfully", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documentation for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred", new[] { ex.Message }, 500));
            }
        }
        #endregion

        #region Prescription
        [HttpPost("{appointmentId}/prescription")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PrescriptionResponse>>> CreatePrescription(Guid appointmentId, [FromBody] CreatePrescriptionRequest request)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
            }

            try
            {
                // Update request with appointmentId and doctorId
                request.AppointmentId = appointmentId;
                request.DoctorId = doctorId;

                var prescription = await _prescriptionService.CreatePrescriptionAsync(request);
                return CreatedAtAction(
                    nameof(GetPrescription),
                    new { appointmentId },
                    ApiResponse<PrescriptionResponse>.Success(prescription, "Prescription created successfully", 201)
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred", new[] { ex.Message }, 500));
            }
        }

        [HttpGet("{appointmentId}/prescription")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PrescriptionResponse>>> GetPrescription(Guid appointmentId)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
            }

            try
            {
                var prescription = await _prescriptionService.GetPrescriptionByAppointmentIdAsync(appointmentId);
                if (prescription == null)
                {
                    return NotFound(ApiResponse<object>.Failure("No prescription for this appointment", statusCode: 404));
                }

                return Ok(ApiResponse<PrescriptionResponse>.Success(prescription, "Prescription retrieved successfully", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred", new[] { ex.Message }, 500));
            }
        }
        #endregion

    }
}
