using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Appointment;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Application.DTOs.Responses.Prescription;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Enums.Appointments;
using System.Security.Claims;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Doctor;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Patient")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IAppointmentService _appointmentService;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(
            IPatientService patientService, 
            IAppointmentService appointmentService, 
            IFileUploadService fileUploadService,
            ILogger<PatientsController> logger)
        {
            _patientService = patientService;
            _appointmentService = appointmentService;
            _fileUploadService = fileUploadService;
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

        private bool IsAccessingOwnData(Guid patientId)
        {
            var currentUserId = GetCurrentPatientId();
            return currentUserId == patientId;
        }

        private string? GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        #endregion

        #region Profile Management (Old - Deprecated, use me/profile instead)

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PatientResponse>> CreatePatient([FromBody] CreatePatientRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid create patient request");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Create patient request by admin");

            try
            {
                var patient = await _patientService.CreatePatientAsync(request);
                _logger.LogInformation("Patient created successfully: {PatientId}", patient.Id);
                return CreatedAtAction(nameof(GetMyPersonalInfo), null, patient);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create patient: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                return StatusCode(500, new { Message = "An error occurred while creating the patient" });
            }
        }

        // Old PUT /api/Patients/me endpoint removed - use PUT /api/Patients/me/profile instead

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeletePatient(Guid id)
        {
            _logger.LogInformation("Delete patient request by admin for patient: {PatientId}", id);

            try
            {
                var result = await _patientService.DeletePatientAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Patient not found for deletion: {PatientId}", id);
                    return NotFound(new { Message = $"Patient with ID {id} not found" });
                }

                _logger.LogInformation("Patient deleted successfully: {PatientId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient: {PatientId}", id);
                return StatusCode(500, new { Message = "An unexpected error occurred while deleting patient" });
            }
        }

        [HttpPost("{id}/restore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RestorePatient(Guid id)
        {
            _logger.LogInformation("Restore patient request for patient: {PatientId}", id);

            try
            {
                var result = await _patientService.RestorePatientAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Patient not found for restoration: {PatientId}", id);
                    return NotFound(new { Message = $"Patient with ID {id} not found or not deleted" });
                }

                _logger.LogInformation("Patient restored successfully: {PatientId}", id);
                return Ok(new { Message = "Patient restored successfully", PatientId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring patient: {PatientId}", id);
                return StatusCode(500, new { Message = "An unexpected error occurred while restoring patient" });
            }
        }
        #endregion

        #region Profile & Address & Medical Record (New Endpoints for Frontend)

        [HttpGet("me/profile")]
        [ProducesResponseType(typeof(ApiResponse<PatientResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PatientResponse>>> GetMyPersonalInfo()
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access patient profile");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get personal info request for patient: {PatientId}", currentPatientId);

            try
            {
                var patient = await _patientService.GetPatientByIdAsync(currentPatientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient profile not found for patient: {PatientId}", currentPatientId);
                    return NotFound(ApiResponse<object>.Failure(
                        "تعذر العثور على المعلومات الشخصية",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Personal info retrieved successfully for patient: {PatientId}", currentPatientId);
                return Ok(ApiResponse<PatientResponse>.Success(
                    patient,
                    "تم جلب المعلومات بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving personal info for patient: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب المعلومات",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// تحديث البيانات الشخصية للمريض (Partial Update)
        /// بيحدث بس الحاجات اللي انت بعتها، مش كل الـ fields
        /// 
        /// مثال: لو عايز تحدث الاسم الأول بس:
        /// { "firstName": "أحمد" }
        /// 
        /// أو لو عايز تحدث الاسم والتليفون:
        /// { "firstName": "أحمد", "phoneNumber": "01234567890" }
        /// 
        /// ملاحظة: الصورة الشخصية والعنوان ليهم endpoints منفصلة
        /// </summary>
        [HttpPut("me/profile")]
        [ProducesResponseType(typeof(ApiResponse<PatientResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PatientResponse>>> UpdateMyPersonalInfo([FromBody] UpdatePatientRequest request)
        {
            var currentPatientId = GetCurrentPatientId();

            // Validate authentication
            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update personal info");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            // Validate request
            if (request == null)
            {
                _logger.LogWarning("Null request received for patient: {PatientId}", currentPatientId);
                return BadRequest(ApiResponse<object>.Failure(
                    "البيانات المرسلة فارغة",
                    statusCode: 400
                ));
            }

            // Validate model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update personal info request for patient: {PatientId}", currentPatientId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "بيانات غير صحيحة",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Update personal info request for patient: {PatientId}", currentPatientId);

            try
            {
                var patient = await _patientService.UpdatePatientAsync(currentPatientId, request);
                _logger.LogInformation("Personal info updated successfully: {PatientId}", currentPatientId);
                
                return Ok(ApiResponse<PatientResponse>.Success(
                    patient,
                    "تم تحديث المعلومات بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error for patient: {PatientId}", currentPatientId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation error updating personal info: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء تحديث المعلومات",
                    new[] { ex.Message },
                    500
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating personal info: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ غير متوقع",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// الحصول على عنوان المريض
        /// </summary>
        [HttpGet("me/address")]
        [ProducesResponseType(typeof(ApiResponse<AddressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AddressResponse?>>> GetMyAddress()
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access address");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get address request for patient: {PatientId}", currentPatientId);

            try
            {
                var address = await _patientService.GetPatientAddressAsync(currentPatientId);
                
                if (address == null)
                {
                    _logger.LogInformation("No address found for patient: {PatientId}", currentPatientId);
                    return Ok(ApiResponse<AddressResponse?>.Success(
                        null,
                        "لا يوجد عنوان مسجل"
                    ));
                }

                _logger.LogInformation("Address retrieved successfully for patient: {PatientId}", currentPatientId);
                return Ok(ApiResponse<AddressResponse?>.Success(
                    address,
                    "تم جلب العنوان بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found: {PatientId}", currentPatientId);
                return NotFound(ApiResponse<object>.Failure(
                    "المريض غير موجود",
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address for patient: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب العنوان",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// تحديث أو إنشاء عنوان المريض (Partial Update)
        /// لو العنوان موجود هيتحدث، لو مش موجود هيتعمل create
        /// بيحدث بس الحاجات اللي انت بعتها
        /// 
        /// مثال: لو عايز تحدث الشارع بس:
        /// { "street": "شارع الجامعة" }
        /// </summary>
        [HttpPut("me/address")]
        [ProducesResponseType(typeof(ApiResponse<AddressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AddressResponse>>> UpdateMyAddress([FromBody] UpdateAddressRequest request)
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update address");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            if (request == null)
            {
                _logger.LogWarning("Null address request for patient: {PatientId}", currentPatientId);
                return BadRequest(ApiResponse<object>.Failure(
                    "البيانات المرسلة فارغة",
                    statusCode: 400
                ));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update address request for patient: {PatientId}", currentPatientId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "بيانات غير صحيحة",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Update/Create address request for patient: {PatientId}", currentPatientId);

            try
            {
                // Check if address exists first
                var existingAddress = await _patientService.GetPatientAddressAsync(currentPatientId);
                bool isCreating = existingAddress == null;

                // Update or create address
                var address = await _patientService.UpdatePatientAddressAsync(currentPatientId, request);
                
                var message = isCreating ? "تم إنشاء العنوان بنجاح" : "تم تحديث العنوان بنجاح";
                _logger.LogInformation("{Action} address for patient: {PatientId}", isCreating ? "Created" : "Updated", currentPatientId);
                
                return Ok(ApiResponse<AddressResponse>.Success(
                    address,
                    message
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found: {PatientId}", currentPatientId);
                return NotFound(ApiResponse<object>.Failure(
                    "المريض غير موجود",
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating/creating address: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء حفظ العنوان",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("me/medical-record")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<MedicalRecordResponse?>>> GetMyMedicalRecord()
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access medical record");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get medical record request for patient: {PatientId}", currentPatientId);

            try
            {
                var medicalRecord = await _patientService.GetPatientMedicalRecordAsync(currentPatientId);
                
                _logger.LogInformation("Medical record retrieved for patient: {PatientId}", currentPatientId);
                return Ok(ApiResponse<MedicalRecordResponse?>.Success(
                    medicalRecord,
                    medicalRecord != null ? "تم جلب الملف الطبي بنجاح" : "لا يوجد ملف طبي مسجل"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical record for patient: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب الملف الطبي",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("me/medical-record")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<MedicalRecordResponse>>> UpdateMyMedicalRecord([FromBody] UpdateMedicalRecordRequest request)
        {
            var currentPatientId = GetCurrentPatientId();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update medical record request for patient: {PatientId}", currentPatientId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "بيانات غير صحيحة",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Update medical record request for patient: {PatientId}", currentPatientId);

            try
            {
                var medicalRecord = await _patientService.UpdatePatientMedicalRecordAsync(currentPatientId, request);
                _logger.LogInformation("Medical record updated successfully: {PatientId}", currentPatientId);
                return Ok(ApiResponse<MedicalRecordResponse>.Success(
                    medicalRecord,
                    "تم تحديث الملف الطبي بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found: {PatientId}", currentPatientId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medical record: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء تحديث الملف الطبي",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Query & Search
        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PatientResponse>> GetPatientByEmail(string email)
        {
            _logger.LogInformation("Get patient by email request: {Email}", email);

            try
            {
                var patient = await _patientService.GetPatientByEmailAsync(email);
                if (patient == null)
                {
                    _logger.LogWarning("Patient not found with email: {Email}", email);
                    return NotFound(new { Message = $"Patient with email {email} not found" });
                }

                _logger.LogInformation("Patient found with email: {Email}", email);
                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient by email: {Email}", email);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving patient" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<PatientResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PatientResponse>>> GetAllPatients([FromQuery] bool includeDeleted = false)
        {
            _logger.LogInformation("Get all patients request, IncludeDeleted: {IncludeDeleted}", includeDeleted);

            try
            {
                var patients = await _patientService.GetAllPatientsAsync(includeDeleted);
                _logger.LogInformation("Retrieved {Count} patients", patients.Count());
                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all patients");
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving patients" });
            }
        }

        [HttpGet("paginated")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PaginatedResponse<PatientResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResponse<PatientResponse>>> GetPaginatedPatients([FromQuery] PaginationParams request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid pagination request");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Get paginated patients request, Page: {Page}, PageSize: {PageSize}", request.PageNumber, request.PageSize);

            try
            {
                var result = await _patientService.GetPaginatedPatientsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated patients");
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving patients" });
            }
        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PaginatedResponse<PatientResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResponse<PatientResponse>>> SearchPatients([FromQuery] SearchTermPatientsRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid search patients request");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Search patients request with term: {SearchTerm}", request.SearchTerm);

            try
            {
                var result = await _patientService.SearchPatientsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching patients");
                return StatusCode(500, new { Message = "An unexpected error occurred while searching patients" });
            }
        }

        [HttpGet("with-medical-history")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<PatientResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PatientResponse>>> GetPatientsWithMedicalHistory()
        {
            _logger.LogInformation("Get patients with medical history request");

            try
            {
                var patients = await _patientService.GetPatientsWithMedicalHistoryAsync();
                _logger.LogInformation("Retrieved {Count} patients with medical history", patients.Count());
                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients with medical history");
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving patients" });
            }
        }

        [HttpGet("check-email/{email}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CheckEmailUnique(string email)
        {
            _logger.LogInformation("Check email uniqueness request: {Email}", email);

            try
            {
                var isUnique = await _patientService.IsEmailUniqueAsync(email);
                _logger.LogInformation("Email {Email} is unique: {IsUnique}", email, isUnique);
                return Ok(new { Email = email, IsUnique = isUnique });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email uniqueness: {Email}", email);
                return StatusCode(500, new { Message = "An unexpected error occurred while checking email" });
            }
        }

        [HttpGet("count")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetTotalPatientsCount([FromQuery] bool includeDeleted = false)
        {
            _logger.LogInformation("Get total patients count request, IncludeDeleted: {IncludeDeleted}", includeDeleted);

            try
            {
                var count = await _patientService.GetTotalPatientsCountAsync(includeDeleted);
                _logger.LogInformation("Total patients count: {Count}", count);
                return Ok(new { TotalCount = count, IncludeDeleted = includeDeleted });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients count");
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving count" });
            }
        }

        [HttpGet("current/{userId}")]
        [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PatientResponse>> GetCurrentPatient(Guid userId)
        {
            _logger.LogInformation("Get current patient request for user: {UserId}", userId);

            try
            {
                var patient = await _patientService.GetCurrentPatientAsync(userId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient not found for user: {UserId}", userId);
                    return NotFound(new { Message = $"Patient with ID {userId} not found" });
                }

                _logger.LogInformation("Current patient retrieved for user: {UserId}", userId);
                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current patient for user: {UserId}", userId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving patient" });
            }
        }
        #endregion

        #region Medical History
        [HttpGet("me/medical-history")]
        [ProducesResponseType(typeof(IEnumerable<MedicalHistoryItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MedicalHistoryItemResponse>>> GetPatientMedicalHistory()
        {
            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Get medical history request for patient: {PatientId}", currentPatientId);

            try
            {
                var medicalHistory = await _patientService.GetPatientMedicalHistoryAsync(currentPatientId);
                _logger.LogInformation("Retrieved {Count} medical history items for patient: {PatientId}", medicalHistory.Count(), currentPatientId);
                return Ok(medicalHistory);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for medical history: {PatientId}", currentPatientId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical history for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving medical history" });
            }
        }

        [HttpPost("me/medical-history")]
        [ProducesResponseType(typeof(MedicalHistoryItemResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicalHistoryItemResponse>> AddMedicalHistoryItem(
            [FromBody] CreateMedicalHistoryItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid add medical history request");
                return BadRequest(ModelState);
            }

            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Add medical history item request for patient: {PatientId}", currentPatientId);

            try
            {
                var item = await _patientService.AddMedicalHistoryItemAsync(currentPatientId, request);
                _logger.LogInformation("Medical history item added successfully for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, item.Id);
                return CreatedAtAction(
                    nameof(GetPatientMedicalHistory),
                    null,
                    item);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for adding medical history: {PatientId}", currentPatientId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding medical history item for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while adding medical history item" });
            }
        }

        [HttpPut("me/medical-history/{itemId}")]
        [ProducesResponseType(typeof(MedicalHistoryItemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicalHistoryItemResponse>> UpdateMedicalHistoryItem(
            Guid itemId,
            [FromBody] UpdateMedicalHistoryItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update medical history request for ItemId: {ItemId}", itemId);
                return BadRequest(ModelState);
            }

            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Update medical history item request for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);

            try
            {
                var item = await _patientService.UpdateMedicalHistoryItemAsync(currentPatientId, itemId, request);
                _logger.LogInformation("Medical history item updated successfully for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);
                return Ok(item);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Medical history item not found for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medical history item for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);
                return StatusCode(500, new { Message = "An unexpected error occurred while updating medical history item" });
            }
        }

        [HttpDelete("me/medical-history/{itemId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteMedicalHistoryItem(Guid itemId)
        {
            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Delete medical history item request for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);

            try
            {
                var result = await _patientService.DeleteMedicalHistoryItemAsync(currentPatientId, itemId);
                if (!result)
                {
                    _logger.LogWarning("Medical history item not found for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);
                    return NotFound(new { Message = "Medical history item not found" });
                }

                _logger.LogInformation("Medical history item deleted successfully for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting medical history item for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);
                return StatusCode(500, new { Message = "An unexpected error occurred while deleting medical history item" });
            }
        }
        #endregion

        #region Appointments
        [HttpGet("me/appointments")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AppointmentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
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

        [HttpGet("me/appointments/{appointmentId}")]
        [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        [HttpPost("me/appointments")]
        [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        [HttpPut("me/appointments/{appointmentId}")]
        [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        [HttpPatch("me/appointments/{appointmentId}/cancel")]
        [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        [HttpPatch("me/appointments/{appointmentId}/reschedule")]
        [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        [HttpDelete("me/appointments/{appointmentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        [HttpGet("me/appointments/upcoming")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        [HttpGet("me/appointments/past")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        [HttpGet("me/appointments/status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        [HttpGet("me/appointments/count")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        [HttpGet("me/appointments/check-availability")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        [HttpGet("me/appointments/date-range")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        #endregion

        #region Prescriptions
        [HttpGet("me/prescriptions")]
        [ProducesResponseType(typeof(IEnumerable<PrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PrescriptionResponse>>> GetPatientPrescriptions()
        {
            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Get prescriptions request for patient: {PatientId}", currentPatientId);

            try
            {
                var prescriptions = await _patientService.GetPatientPrescriptionsAsync(currentPatientId);
                _logger.LogInformation("Retrieved {Count} prescriptions for patient: {PatientId}", prescriptions.Count(), currentPatientId);
                return Ok(prescriptions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for prescriptions: {PatientId}", currentPatientId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescriptions for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving prescriptions" });
            }
        }

        [HttpGet("me/prescriptions/active")]
        [ProducesResponseType(typeof(IEnumerable<PrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PrescriptionResponse>>> GetActivePrescriptions()
        {
            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Get active prescriptions request for patient: {PatientId}", currentPatientId);

            try
            {
                var prescriptions = await _patientService.GetActivePrescriptionsAsync(currentPatientId);
                _logger.LogInformation("Retrieved {Count} active prescriptions for patient: {PatientId}", prescriptions.Count(), currentPatientId);
                return Ok(prescriptions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for active prescriptions: {PatientId}", currentPatientId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active prescriptions for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving active prescriptions" });
            }
        }

        [HttpGet("me/prescriptions/{prescriptionId}")]
        [ProducesResponseType(typeof(PrescriptionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PrescriptionResponse>> GetPrescriptionById(Guid prescriptionId)
        {
            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Get prescription by ID request for patient: {PatientId}, PrescriptionId: {PrescriptionId}", currentPatientId, prescriptionId);

            try
            {
                var prescription = await _patientService.GetPrescriptionByIdAsync(currentPatientId, prescriptionId);
                if (prescription == null)
                {
                    _logger.LogWarning("Prescription not found: {PrescriptionId} for patient: {PatientId}", prescriptionId, currentPatientId);
                    return NotFound(new { Message = $"Prescription with ID {prescriptionId} not found" });
                }

                _logger.LogInformation("Prescription retrieved successfully: {PrescriptionId} for patient: {PatientId}", prescriptionId, currentPatientId);
                return Ok(prescription);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for prescription: {PatientId}", currentPatientId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescription {PrescriptionId} for patient: {PatientId}", prescriptionId, currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving prescription" });
            }
        }
        #endregion

        #region Lab Orders
        [HttpGet("me/lab-orders")]
        [ProducesResponseType(typeof(IEnumerable<LabOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LabOrderResponse>>> GetPatientLabOrders()
        {
            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Get lab orders request for patient: {PatientId}", currentPatientId);

            try
            {
                var labOrders = await _patientService.GetPatientLabOrdersAsync(currentPatientId);
                _logger.LogInformation("Retrieved {Count} lab orders for patient: {PatientId}", labOrders.Count(), currentPatientId);
                return Ok(labOrders);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for lab orders: {PatientId}", currentPatientId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lab orders for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving lab orders" });
            }
        }

        [HttpGet("me/lab-orders/pending")]
        [ProducesResponseType(typeof(IEnumerable<LabOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LabOrderResponse>>> GetPendingLabOrders()
        {
            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Get pending lab orders request for patient: {PatientId}", currentPatientId);

            try
            {
                var labOrders = await _patientService.GetPendingLabOrdersAsync(currentPatientId);
                _logger.LogInformation("Retrieved {Count} pending lab orders for patient: {PatientId}", labOrders.Count(), currentPatientId);
                return Ok(labOrders);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for pending lab orders: {PatientId}", currentPatientId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending lab orders for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving pending lab orders" });
            }
        }

        [HttpGet("me/lab-orders/{orderId}")]
        [ProducesResponseType(typeof(LabOrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LabOrderResponse>> GetLabOrderById(Guid orderId)
        {
            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Get lab order by ID request for patient: {PatientId}, OrderId: {OrderId}", currentPatientId, orderId);

            try
            {
                var labOrder = await _patientService.GetLabOrderByIdAsync(currentPatientId, orderId);
                if (labOrder == null)
                {
                    _logger.LogWarning("Lab order not found: {OrderId} for patient: {PatientId}", orderId, currentPatientId);
                    return NotFound(new { Message = $"Lab order with ID {orderId} not found" });
                }

                _logger.LogInformation("Lab order retrieved successfully: {OrderId} for patient: {PatientId}", orderId, currentPatientId);
                return Ok(labOrder);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for lab order: {PatientId}", currentPatientId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lab order {OrderId} for patient: {PatientId}", orderId, currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving lab order" });
            }
        }
        #endregion

        #region Address Management (Old - Deprecated, use Profile & Address & Medical Record section)
        // Old endpoints removed to avoid conflicts with new unified endpoints
        #endregion

        #region Profile Image
        /// <summary>
        /// تحديث الصورة الشخصية للمريض
        /// بيحذف الصورة القديمة من Cloudinary (لو موجودة) ويرفع الصورة الجديدة
        /// </summary>
        [HttpPut("me/profile-image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateProfileImage([FromForm] UpdateProfileImageRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update profile image request");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "بيانات غير صحيحة",
                    errors,
                    400
                ));
            }

            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update profile image");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Update profile image request for patient: {PatientId}", currentPatientId);

            try
            {
                // Get patient to check for old image
                var patient = await _patientService.GetPatientByIdAsync(currentPatientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient not found for profile image update: {PatientId}", currentPatientId);
                    return NotFound(ApiResponse<object>.Failure(
                        "المريض غير موجود",
                        statusCode: 404
                    ));
                }

                // Delete old image from Cloudinary if exists
                if (!string.IsNullOrWhiteSpace(patient.ProfileImageUrl))
                {
                    try
                    {
                        await _fileUploadService.DeleteFileAsync(patient.ProfileImageUrl);
                        _logger.LogInformation("Deleted old profile image from Cloudinary for patient: {PatientId}", currentPatientId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old image from Cloudinary, continuing with upload");
                        // Continue even if deletion fails
                    }
                }

                // Upload the new profile image
                var uploadResult = await _fileUploadService.UploadProfileImageAsync(request.ProfileImage, currentPatientId.ToString());
                _logger.LogInformation("Uploaded new profile image to Cloudinary for patient: {PatientId}", currentPatientId);
                
                // Update patient's profile image URL in database
                var result = await _patientService.UpdateProfileImageAsync(currentPatientId, uploadResult.FileUrl);
                
                if (!result)
                {
                    _logger.LogWarning("Failed to update profile image URL in database: {PatientId}", currentPatientId);
                    return StatusCode(500, ApiResponse<object>.Failure(
                        "فشل تحديث الصورة في قاعدة البيانات",
                        statusCode: 500
                    ));
                }
                
                _logger.LogInformation("Profile image updated successfully for patient: {PatientId}", currentPatientId);
                return Ok(ApiResponse<object>.Success(
                    new { 
                        PatientId = currentPatientId, 
                        ProfileImageUrl = uploadResult.FileUrl 
                    },
                    "تم تحديث الصورة الشخصية بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for profile image update: {PatientId}", currentPatientId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile image for patient: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء تحديث الصورة الشخصية",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// حذف الصورة الشخصية للمريض
        /// بيحذف الصورة من Cloudinary وبيحذف الـ URL من قاعدة البيانات
        /// </summary>
        [HttpDelete("me/profile-image")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> RemoveProfileImage()
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to remove profile image");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Remove profile image request for patient: {PatientId}", currentPatientId);

            try
            {
                // Get patient to check if they have a profile image
                var patient = await _patientService.GetPatientByIdAsync(currentPatientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient not found for profile image removal: {PatientId}", currentPatientId);
                    return NotFound(ApiResponse<object>.Failure(
                        "المريض غير موجود",
                        statusCode: 404
                    ));
                }

                // Check if patient has a profile image
                if (string.IsNullOrWhiteSpace(patient.ProfileImageUrl))
                {
                    _logger.LogInformation("Patient {PatientId} does not have a profile image to remove", currentPatientId);
                    return Ok(ApiResponse<object>.Success(
                        new { PatientId = currentPatientId },
                        "لا توجد صورة شخصية لحذفها"
                    ));
                }

                // Delete from Cloudinary
                try
                {
                    await _fileUploadService.DeleteFileAsync(patient.ProfileImageUrl);
                    _logger.LogInformation("Deleted profile image from Cloudinary for patient: {PatientId}", currentPatientId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete image from Cloudinary, continuing with database update");
                    // Continue even if Cloudinary deletion fails
                }

                // Remove from database
                var result = await _patientService.RemoveProfileImageAsync(currentPatientId);
                if (!result)
                {
                    _logger.LogWarning("Failed to remove profile image URL from database: {PatientId}", currentPatientId);
                    return StatusCode(500, ApiResponse<object>.Failure(
                        "فشل حذف الصورة من قاعدة البيانات",
                        statusCode: 500
                    ));
                }

                _logger.LogInformation("Profile image removed successfully for patient: {PatientId}", currentPatientId);
                return Ok(ApiResponse<object>.Success(
                    new { PatientId = currentPatientId },
                    "تم حذف الصورة الشخصية بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing profile image for patient: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء حذف الصورة الشخصية",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion
    }
}
