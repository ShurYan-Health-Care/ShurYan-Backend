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
    /// <summary>
    /// Controller مسؤول عن عمليات الحجز (Booking System)
    /// </summary>
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

        #region Booking System - Frontend Integration

        /// <summary>
        /// 5️⃣ POST Book New Appointment - حجز موعد جديد
        /// </summary>
        /// <remarks>
        /// **متطلبات الـ Request:**
        /// 
        ///     POST /api/Appointments/book
        ///     Authorization: Bearer {token}
        ///     Content-Type: application/json
        ///     
        ///     {
        ///       "doctorId": "123e4567-e89b-12d3-a456-426614174000",
        ///       "appointmentDate": "2025-01-15",
        ///       "appointmentTime": "09:00",
        ///       "consultationType": 0
        ///     }
        /// 
        /// **Validation Rules:**
        /// - ✅ doctorId: يجب أن يكون موجود
        /// - ✅ appointmentDate: صيغة YYYY-MM-DD وليس في الماضي
        /// - ✅ appointmentTime: صيغة 24-hour (HH:mm)
        /// - ✅ consultationType: 0 (كشف عادي) أو 1 (إعادة كشف)
        /// - ✅ الفترة الزمنية يجب أن تكون متاحة (غير محجوزة)
        /// - ✅ الفترة الزمنية يجب أن تكون ضمن ساعات عمل الدكتور
        /// 
        /// **Success Response (201):**
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "تم حجز الموعد بنجاح",
        ///       "data": {
        ///         "id": "uuid",
        ///         "doctorId": "uuid",
        ///         "patientId": "uuid",
        ///         "appointmentDate": "2025-01-15",
        ///         "appointmentTime": "09:00",
        ///         "consultationType": 0,
        ///         "status": "Confirmed",
        ///         "totalAmount": 300,
        ///         "createdAt": "2025-01-10T10:30:00Z"
        ///       }
        ///     }
        /// 
        /// **Error Response - Slot Already Booked (409):**
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "هذا الموعد محجوز بالفعل",
        ///       "errors": ["الفترة الزمنية 09:00 محجوزة"]
        ///     }
        /// 
        /// **Error Response - Past Date (400):**
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "لا يمكن حجز موعد في الماضي",
        ///       "errors": []
        ///     }
        /// </remarks>
        [HttpPost("book")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(ApiResponse<BookedAppointmentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<BookedAppointmentResponse>>> BookAppointment(
            [FromBody] BookAppointmentRequest request)
        {
            var patientId = GetCurrentPatientId();

            if (patientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized booking attempt - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
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
                    ApiResponse<BookedAppointmentResponse>.Success(
                        bookedAppointment,
                        "تم حجز الموعد بنجاح",
                        201
                    )
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while booking appointment for Patient {PatientId}", patientId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    new[] { ex.Message },
                    400
                ));
            }
            catch (InvalidOperationException ex)
            {
                // Check if it's a "slot already booked" error
                if (ex.Message.Contains("محجوزة") || ex.Message.Contains("booked"))
                {
                    _logger.LogWarning(ex, "Appointment slot conflict for Patient {PatientId}", patientId);
                    return Conflict(ApiResponse<object>.Failure(
                        "هذا الموعد محجوز بالفعل",
                        new[] { ex.Message },
                        409
                    ));
                }

                // Other business logic errors (e.g., past date, doctor hasn't set pricing)
                _logger.LogWarning(ex, "Business logic error while booking appointment for Patient {PatientId}", patientId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    new[] { ex.Message },
                    400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while booking appointment for Patient {PatientId}", patientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ غير متوقع أثناء حجز الموعد",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Get Appointment Details

        /// <summary>
        /// الحصول على تفاصيل الموعد
        /// GET /api/Appointments/{appointmentId}
        /// </summary>
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
                        "الموعد غير موجود",
                        new[] { "Appointment not found" },
                        404
                    ));
                }

                // التحقق من الصلاحيات - الدكتور أو المريض فقط
                var userId = GetCurrentDoctorId(); // نفس الـ method
                var isDoctor = IsDoctor();
                var isPatient = IsPatient();

                if (isDoctor && appointment.DoctorId != userId)
                {
                    return Unauthorized(ApiResponse<object>.Failure(
                        "غير مصرح لك بالوصول لهذا الموعد",
                        new[] { "Unauthorized access" },
                        401
                    ));
                }

                if (isPatient && appointment.PatientId != userId)
                {
                    return Unauthorized(ApiResponse<object>.Failure(
                        "غير مصرح لك بالوصول لهذا الموعد",
                        new[] { "Unauthorized access" },
                        401
                    ));
                }

                return Ok(ApiResponse<AppointmentResponse>.Success(
                    appointment,
                    "تم الحصول على تفاصيل الموعد بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء الحصول على تفاصيل الموعد",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Session Management

        /// <summary>
        /// بدء جلسة كشف جديدة
        /// POST /api/Appointments/{appointmentId}/start-session
        /// </summary>
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
                    ApiResponse<SessionResponse>.Success(session, "تم بدء الجلسة بنجاح", 201)
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
                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ غير متوقع", new[] { ex.Message }, 500));
            }
        }

        /// <summary>
        /// الحصول على الجلسة النشطة للموعد
        /// GET /api/Appointments/{appointmentId}/session
        /// </summary>
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
                    return NotFound(ApiResponse<object>.Failure("لا توجد جلسة نشطة لهذا الموعد", statusCode: 404));
                }

                return Ok(ApiResponse<SessionResponse>.Success(session, "تم استرجاع الجلسة بنجاح", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active session for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ غير متوقع", new[] { ex.Message }, 500));
            }
        }

        /// <summary>
        /// إنهاء الجلسة النشطة
        /// POST /api/Appointments/{appointmentId}/end-session
        /// </summary>
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
                return Ok(ApiResponse<EndSessionResponse>.Success(result, "تم إنهاء الجلسة بنجاح", 200));
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
                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ غير متوقع", new[] { ex.Message }, 500));
            }
        }

        #endregion

        #region Documentation

        /// <summary>
        /// حفظ أو تحديث توثيق الكشف
        /// POST/PUT /api/Appointments/{appointmentId}/documentation
        /// </summary>
        [HttpPost("{appointmentId}/documentation")]
        [HttpPut("{appointmentId}/documentation")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<DocumentationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<DocumentationResponse>>> SaveDocumentation(
            Guid appointmentId,
            [FromBody] SaveDocumentationRequest request)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
            }

            try
            {
                var documentation = await _documentationService.SaveDocumentationAsync(appointmentId, doctorId, request);
                return Ok(ApiResponse<DocumentationResponse>.Success(documentation, "تم حفظ التوثيق بنجاح", 200));
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
                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ غير متوقع", new[] { ex.Message }, 500));
            }
        }

        /// <summary>
        /// الحصول على توثيق الكشف
        /// GET /api/Appointments/{appointmentId}/documentation
        /// </summary>
        [HttpGet("{appointmentId}/documentation")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<DocumentationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DocumentationResponse>>> GetDocumentation(Guid appointmentId)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
            }

            try
            {
                var documentation = await _documentationService.GetDocumentationAsync(appointmentId, doctorId);
                if (documentation == null)
                {
                    return NotFound(ApiResponse<object>.Failure("لا يوجد توثيق لهذا الموعد", statusCode: 404));
                }

                return Ok(ApiResponse<DocumentationResponse>.Success(documentation, "تم استرجاع التوثيق بنجاح", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documentation for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ غير متوقع", new[] { ex.Message }, 500));
            }
        }

        #endregion

        #region Prescription

        /// <summary>
        /// إنشاء روشتة جديدة
        /// POST /api/Appointments/{appointmentId}/prescription
        /// </summary>
        [HttpPost("{appointmentId}/prescription")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PrescriptionResponse>>> CreatePrescription(
            Guid appointmentId,
            [FromBody] CreatePrescriptionRequest request)
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
                    ApiResponse<PrescriptionResponse>.Success(prescription, "تم إنشاء الروشتة بنجاح", 201)
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ غير متوقع", new[] { ex.Message }, 500));
            }
        }

        /// <summary>
        /// الحصول على الروشتة
        /// GET /api/Appointments/{appointmentId}/prescription
        /// </summary>
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
                    return NotFound(ApiResponse<object>.Failure("لا توجد روشتة لهذا الموعد", statusCode: 404));
                }

                return Ok(ApiResponse<PrescriptionResponse>.Success(prescription, "تم استرجاع الروشتة بنجاح", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ غير متوقع", new[] { ex.Message }, 500));
            }
        }

        #endregion

        #region Lab Tests

        /// <summary>
        /// طلب تحاليل طبية جديدة
        /// POST /api/Appointments/{appointmentId}/lab-tests
        /// </summary>
        [HttpPost("{appointmentId}/lab-tests")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<LabTestsResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<LabTestsResponse>>> RequestLabTests(
            Guid appointmentId,
            [FromBody] RequestLabTestsRequest request)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
            }

            try
            {
                var labTests = await _labTestService.RequestLabTestsAsync(appointmentId, doctorId, request);
                return CreatedAtAction(
                    nameof(GetLabTests),
                    new { appointmentId },
                    ApiResponse<LabTestsResponse>.Success(labTests, "تم طلب التحاليل بنجاح", 201)
                );
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
                _logger.LogError(ex, "Error requesting lab tests for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ غير متوقع", new[] { ex.Message }, 500));
            }
        }

        /// <summary>
        /// الحصول على طلبات التحاليل
        /// GET /api/Appointments/{appointmentId}/lab-tests
        /// </summary>
        [HttpGet("{appointmentId}/lab-tests")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<LabTestsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<LabTestsResponse>>> GetLabTests(Guid appointmentId)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
            }

            try
            {
                var labTests = await _labTestService.GetLabTestsAsync(appointmentId, doctorId);
                if (labTests == null)
                {
                    return NotFound(ApiResponse<object>.Failure("لا توجد تحاليل لهذا الموعد", statusCode: 404));
                }

                return Ok(ApiResponse<LabTestsResponse>.Success(labTests, "تم استرجاع التحاليل بنجاح", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.Failure(ex.Message, new[] { ex.Message }, 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab tests for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ غير متوقع", new[] { ex.Message }, 500));
            }
        }

        #endregion
    }
}
