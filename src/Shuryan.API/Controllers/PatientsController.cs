using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(
            IPatientService patientService,
            ILogger<PatientsController> logger)
        {
            _patientService = patientService;
            _logger = logger;
        }

        #region Admin CRUD Operations

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                return CreatedAtAction(nameof(GetCurrentPatient), new { userId = patient.Id }, patient);
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        #region Admin Query & Search Operations

        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        #region Pharmacy Operations

        /// <summary>
        /// البحث عن أقرب 3 صيدليات للمريض بناءً على عنوانه المسجل أو الإحداثيات المرسلة
        /// يمكن تمرير إحداثيات اختيارية في الـ query parameters إذا لم يكن للمريض عنوان مسجل
        /// </summary>
        [HttpGet("nearby-pharmacies")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(FindNearbyPharmaciesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FindNearbyPharmaciesResponse>> FindNearbyPharmacies(
            [FromQuery] double? latitude = null, 
            [FromQuery] double? longitude = null)
        {
            // الحصول على ID المريض من الـ JWT token
            var userIdClaim = User.FindFirst("sub") ?? 
                             User.FindFirst("id") ?? 
                             User.FindFirst("userId") ?? 
                             User.FindFirst("nameid") ??
                             User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            
            if (userIdClaim == null)
            {
                _logger.LogWarning("No user ID claim found in JWT token. Available claims: {Claims}", 
                    string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                return BadRequest(new { Message = "Invalid patient authentication - no user ID found" });
            }

            if (!Guid.TryParse(userIdClaim.Value, out var patientId))
            {
                _logger.LogWarning("Invalid user ID format in JWT token: {UserId}", userIdClaim.Value);
                return BadRequest(new { Message = "Invalid patient authentication - invalid ID format" });
            }

            _logger.LogInformation("Find nearby pharmacies request for patient: {PatientId}", patientId);

            try
            {
                FindNearbyPharmaciesResponse response;

                // إذا تم تمرير إحداثيات في الـ query، استخدمها مباشرة
                if (latitude.HasValue && longitude.HasValue)
                {
                    _logger.LogInformation("Using provided coordinates: {Latitude}, {Longitude}", latitude.Value, longitude.Value);
                    
                    var request = new FindNearbyPharmaciesRequest
                    {
                        Latitude = latitude.Value,
                        Longitude = longitude.Value
                    };
                    
                    response = await _patientService.FindNearbyPharmaciesAsync(request);
                }
                else
                {
                    // استخدم عنوان المريض
                    response = await _patientService.FindNearbyPharmaciesForPatientAsync(patientId);
                }
                
                _logger.LogInformation("Found {Count} nearby pharmacies for patient {PatientId}", response.TotalFound, patientId);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Patient or patient address not found: {PatientId}", patientId);
                
                // إذا لم يكن للمريض عنوان، اقترح استخدام query parameters
                if (ex.Message.Contains("address"))
                {
                    return NotFound(new { 
                        Message = ex.Message,
                        Suggestion = "You can provide coordinates as query parameters: ?latitude=30.0444&longitude=31.2357"
                    });
                }
                
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding nearby pharmacies for patient: {PatientId}", patientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while finding nearby pharmacies" });
            }
        }

        /// <summary>
        /// جلب كل ردود الصيدليات على روشتة معينة
        /// </summary>
        [HttpGet("me/prescriptions/{prescriptionId}/pharmacy-responses")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(PrescriptionPharmacyResponsesView), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PrescriptionPharmacyResponsesView>> GetPrescriptionPharmacyResponses(Guid prescriptionId)
        {
            var patientId = GetCurrentPatientId();

            if (patientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to get prescription pharmacy responses");
                return Unauthorized(new { Message = "غير مصرح لك بالوصول" });
            }

            _logger.LogInformation("Getting pharmacy responses for prescription {PrescriptionId} and patient {PatientId}", 
                prescriptionId, patientId);

            try
            {
                var response = await _patientService.GetPrescriptionPharmacyResponsesAsync(patientId, prescriptionId);
                
                _logger.LogInformation("Successfully retrieved {Count} pharmacy responses for prescription {PrescriptionId}", 
                    response.TotalPharmacyResponses, prescriptionId);
                
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for prescription pharmacy responses: {PatientId}, {PrescriptionId}", 
                    patientId, prescriptionId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription pharmacy responses for prescription {PrescriptionId}", prescriptionId);
                return StatusCode(500, new { Message = "حدث خطأ أثناء جلب ردود الصيدليات" });
            }
        }

        /// <summary>
        /// إرسال روشتة إلى صيدلية معينة
        /// </summary>
        [HttpPost("me/prescriptions/{prescriptionId}/send-to-pharmacy")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(SendPrescriptionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SendPrescriptionResponse>> SendPrescriptionToPharmacy(
            Guid prescriptionId,
            [FromBody] SendPrescriptionToPharmacyRequest request)
        {
            var patientId = GetCurrentPatientId();

            if (patientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to send prescription to pharmacy");
                return Unauthorized(new { Message = "غير مصرح لك بالوصول" });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid send prescription request for patient: {PatientId}", patientId);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Sending prescription {PrescriptionId} from patient {PatientId} to pharmacy {PharmacyId}", 
                prescriptionId, patientId, request.PharmacyId);

            try
            {
                var response = await _patientService.SendPrescriptionToPharmacyAsync(patientId, prescriptionId, request);
                
                _logger.LogInformation("Successfully sent prescription {PrescriptionId} to pharmacy {PharmacyId}", 
                    prescriptionId, request.PharmacyId);
                
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for send prescription: {PatientId}, {PrescriptionId}, {PharmacyId}", 
                    patientId, prescriptionId, request.PharmacyId);
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for send prescription: {PatientId}, {PrescriptionId}", 
                    patientId, prescriptionId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending prescription {PrescriptionId} to pharmacy {PharmacyId}", 
                    prescriptionId, request.PharmacyId);
                return StatusCode(500, new { Message = "حدث خطأ أثناء إرسال الروشتة" });
            }
        }

        /// <summary>
        /// جلب رد صيدلية واحدة على طلب معين (الـ endpoint القديم - للتوافق مع الإصدارات السابقة)
        /// </summary>
        [HttpGet("me/orders/{orderId}/pharmacy-response")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(PatientPharmacyResponseView), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PatientPharmacyResponseView>> GetPharmacyResponse(Guid orderId)
        {
            var patientId = GetCurrentPatientId();

            if (patientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to get pharmacy response");
                return Unauthorized(new { Message = "غير مصرح لك بالوصول" });
            }

            _logger.LogInformation("Getting pharmacy response for order {OrderId} and patient {PatientId}", orderId, patientId);

            try
            {
                var response = await _patientService.GetPharmacyResponseAsync(patientId, orderId);
                
                _logger.LogInformation("Successfully retrieved pharmacy response for order {OrderId}", orderId);
                
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for pharmacy response: {PatientId}, {OrderId}", patientId, orderId);
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for pharmacy response: {PatientId}, {OrderId}", patientId, orderId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pharmacy response for order {OrderId}", orderId);
                return StatusCode(500, new { Message = "حدث خطأ أثناء جلب رد الصيدلية" });
            }
        }

        #region Helper Methods

        private Guid GetCurrentPatientId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        #endregion

        #endregion
    }
}
