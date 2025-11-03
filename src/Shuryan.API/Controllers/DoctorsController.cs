using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Appointment;
using Shuryan.Application.DTOs.Requests.Doctor;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IDoctorScheduleService _scheduleService;
        private readonly IDoctorServicePricingService _servicePricingService;
        private readonly IAppointmentService _appointmentService;
        private readonly ISessionService _sessionService;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(
            IDoctorService doctorService,
            IDoctorScheduleService scheduleService,
            IDoctorServicePricingService servicePricingService,
            IAppointmentService appointmentService,
            ISessionService sessionService,
            ILogger<DoctorsController> logger)
        {
            _doctorService = doctorService;
            _scheduleService = scheduleService;
            _servicePricingService = servicePricingService;
            _appointmentService = appointmentService;
            _sessionService = sessionService;
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

        private bool IsAccessingOwnData(Guid doctorId)
        {
            var currentUserId = GetCurrentDoctorId();
            return currentUserId == doctorId;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }
        #endregion

        #region Profile Operations - GET

        [HttpGet("me")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<DoctorProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorProfileResponse>>> GetMyProfile()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access doctor profile - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get doctor profile request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var doctor = await _doctorService.GetDoctorProfileAsync(currentDoctorId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor profile not found for doctor: {DoctorId}", currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Doctor with ID {currentDoctorId} not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Doctor profile retrieved successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<DoctorProfileResponse>.Success(
                    doctor,
                    "Profile retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor profile for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving the profile",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<DoctorProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorProfileResponse>>> GetDoctorProfile(Guid id)
        {
            _logger.LogInformation("Get doctor profile request for doctor: {DoctorId}", id);

            try
            {
                var doctor = await _doctorService.GetDoctorProfileAsync(id);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor profile not found for doctor: {DoctorId}", id);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Doctor with ID {id} not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Doctor profile retrieved successfully for doctor: {DoctorId}", id);
                return Ok(ApiResponse<DoctorProfileResponse>.Success(
                    doctor,
                    "Profile retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor profile for doctor: {DoctorId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving the profile",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("profile/personal")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<DoctorPersonalProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorPersonalProfileResponse>>> GetPersonalProfile()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access personal profile - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get personal profile request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var personalProfile = await _doctorService.GetPersonalProfileAsync(currentDoctorId);
                if (personalProfile == null)
                {
                    _logger.LogWarning("Personal profile not found for doctor: {DoctorId}", currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Personal profile not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Personal profile retrieved successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<DoctorPersonalProfileResponse>.Success(
                    personalProfile,
                    "Personal profile retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving personal profile for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving personal profile",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("profile/professional")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<DoctorProfessionalInfoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorProfessionalInfoResponse>>> GetProfessionalInfo()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access professional info - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get professional info request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var professionalInfo = await _doctorService.GetProfessionalInfoAsync(currentDoctorId);
                if (professionalInfo == null)
                {
                    _logger.LogWarning("Professional info not found for doctor: {DoctorId}", currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Professional information not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Professional info retrieved successfully for doctor: {DoctorId} with {DocumentCount} documents", 
                    currentDoctorId, professionalInfo.TotalDocuments);
                return Ok(ApiResponse<DoctorProfessionalInfoResponse>.Success(
                    professionalInfo,
                    "Professional information retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving professional info for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving professional information",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("profile/specialty-experience")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<DoctorSpecialtyExperienceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorSpecialtyExperienceResponse>>> GetSpecialtyExperience()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access specialty and experience - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get specialty and experience request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var specialtyExperience = await _doctorService.GetSpecialtyExperienceAsync(currentDoctorId);
                if (specialtyExperience == null)
                {
                    _logger.LogWarning("Specialty and experience not found for doctor: {DoctorId}", currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Specialty and experience information not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Specialty and experience retrieved successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<DoctorSpecialtyExperienceResponse>.Success(
                    specialtyExperience,
                    "Specialty and experience retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving specialty and experience for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving specialty and experience",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Profile Operations - UPDATE

        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        [ProducesResponseType(typeof(ApiResponse<DoctorProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorProfileResponse>>> UpdateDoctorProfile(Guid id, [FromBody] UpdateDoctorProfileRequest request)
        {
            _logger.LogInformation("Update doctor profile request for doctor: {DoctorId}", id);

            // Ownership check: Doctors can only update their own profile unless they're Admin
            if (!IsAdmin() && !IsAccessingOwnData(id))
            {
                _logger.LogWarning("Forbidden: Doctor {CurrentDoctorId} attempted to update profile of another doctor {DoctorId}",
                    GetCurrentDoctorId(), id);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure(
                    "Doctors can only update their own profile",
                    statusCode: 403
                ));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UpdateDoctorProfile for doctor: {DoctorId}. Errors: {Errors}",
                    id, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var doctor = await _doctorService.UpdateDoctorProfileAsync(id, request);
                _logger.LogInformation("Doctor profile updated successfully for doctor: {DoctorId}", id);
                return Ok(ApiResponse<DoctorProfileResponse>.Success(
                    doctor,
                    "Profile updated successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found for profile update: {DoctorId}", id);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor profile for doctor: {DoctorId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating the profile",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("profile/personal")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<DoctorProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorProfileResponse>>> UpdatePersonalInfo([FromBody] UpdatePersonalInfoRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update personal info - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Update personal info request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UpdatePersonalInfo for doctor: {DoctorId}. Errors: {Errors}",
                    currentDoctorId, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var doctor = await _doctorService.UpdatePersonalInfoAsync(currentDoctorId, request);
                _logger.LogInformation("Personal info updated successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<DoctorProfileResponse>.Success(
                    doctor,
                    "Personal information updated successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found for personal info update: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating personal info for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating personal information",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("profile/specialty-experience")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<DoctorSpecialtyExperienceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorSpecialtyExperienceResponse>>> UpdateSpecialtyExperience([FromBody] UpdateSpecialtyExperienceRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update specialty and experience - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Update specialty and experience request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UpdateSpecialtyExperience for doctor: {DoctorId}. Errors: {Errors}",
                    currentDoctorId, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var result = await _doctorService.UpdateSpecialtyExperienceAsync(currentDoctorId, request);
                _logger.LogInformation("Specialty and experience updated successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<DoctorSpecialtyExperienceResponse>.Success(
                    result,
                    "Specialty and experience updated successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found for specialty and experience update: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating specialty and experience for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating specialty and experience",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("me/profile-image")]
        [Authorize(Roles = "Doctor")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<DoctorProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorProfileResponse>>> UpdateMyProfileImage([FromForm] UpdateProfileImageRequest request)
        {
            // Get current doctor ID from JWT token
            var currentDoctorId = GetCurrentDoctorId();

            _logger.LogInformation("Update profile image request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UpdateProfileImage for doctor: {DoctorId}. Errors: {Errors}",
                    currentDoctorId, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                // Create UpdateDoctorProfileRequest with the profile image
                var updateRequest = new UpdateDoctorProfileRequest
                {
                    ProfileImage = request.ProfileImage
                };

                var doctor = await _doctorService.UpdateDoctorProfileAsync(currentDoctorId, updateRequest);
                _logger.LogInformation("Profile image uploaded successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<DoctorProfileResponse>.Success(
                    doctor,
                    "Profile image uploaded successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found for profile image update: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile image for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating profile image",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Document Operations - GET

        [HttpGet("documents/{documentId}")]
        [Authorize(Roles = "Doctor,Admin")]
        [ProducesResponseType(typeof(ApiResponse<DoctorDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorDocumentResponse>>> GetDocumentById(Guid documentId)
        {
            _logger.LogInformation("Getting document by ID: {DocumentId}", documentId);
            try
            {
                var document = await _doctorService.GetDocumentByIdAsync(documentId);
                if (document == null)
                {
                    _logger.LogWarning("Document not found: {DocumentId}", documentId);
                    return NotFound(ApiResponse<object>.Failure($"Document with ID {documentId} not found", statusCode: 404));
                }

                return Ok(ApiResponse<DoctorDocumentResponse>.Success(document, "Document retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document by ID: {DocumentId}", documentId);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while getting the document", new[] { ex.Message }, 500));
            }
        }

        [HttpGet("me/documents/required")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorDocumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DoctorDocumentResponse>>>> GetMyRequiredDocuments()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to get required documents - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get required documents request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var documents = await _doctorService.GetRequiredDocumentsAsync(currentDoctorId);
                _logger.LogInformation("Retrieved {Count} required documents for doctor: {DoctorId}",
                    documents.Count(), currentDoctorId);

                return Ok(ApiResponse<IEnumerable<DoctorDocumentResponse>>.Success(
                    documents,
                    "Required documents retrieved successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving required documents for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving required documents",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("me/documents/research")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorDocumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DoctorDocumentResponse>>>> GetMyResearchPapers()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to get research papers - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get research papers request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var documents = await _doctorService.GetResearchPapersAsync(currentDoctorId);
                _logger.LogInformation("Retrieved {Count} research papers for doctor: {DoctorId}",
                    documents.Count(), currentDoctorId);

                return Ok(ApiResponse<IEnumerable<DoctorDocumentResponse>>.Success(
                    documents,
                    "Research papers retrieved successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving research papers for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving research papers",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("me/documents/awards")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorDocumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DoctorDocumentResponse>>>> GetMyAwardCertificates()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to get awards/certificates - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get awards/certificates request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var documents = await _doctorService.GetAwardCertificatesAsync(currentDoctorId);
                _logger.LogInformation("Retrieved {Count} awards/certificates for doctor: {DoctorId}",
                    documents.Count(), currentDoctorId);

                return Ok(ApiResponse<IEnumerable<DoctorDocumentResponse>>.Success(
                    documents,
                    "Awards and certificates retrieved successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving awards/certificates for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving awards and certificates",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("me/documents/required")]
        [Authorize(Roles = "Doctor")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<DoctorDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorDocumentResponse>>> UploadOrUpdateRequiredDocument([FromForm] UploadDoctorDocumentRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to upload required document - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Upload/Update required document request for doctor: {DoctorId}, Type: {Type}",
                currentDoctorId, request.Type);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for required document upload. Errors: {Errors}",
                    string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var document = await _doctorService.UploadOrUpdateRequiredDocumentAsync(currentDoctorId, request);
                _logger.LogInformation("Required document {DocumentId} uploaded/updated successfully for doctor {DoctorId}",
                    document.Id, currentDoctorId);

                return Ok(ApiResponse<DoctorDocumentResponse>.Success(
                    document,
                    "Required document uploaded/updated successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request on required document upload for doctor {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on required document upload for doctor {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading/updating required document for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while uploading/updating the required document",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("me/documents/awards")]
        [Authorize(Roles = "Doctor")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<DoctorDocumentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorDocumentResponse>>> UploadAwardCertificate([FromForm] UploadDoctorDocumentRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to upload award/certificate - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Upload award/certificate request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for award upload. Errors: {Errors}",
                    string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var document = await _doctorService.UploadOrUpdateAwardCertificateAsync(currentDoctorId, request);
                _logger.LogInformation("Award/certificate {DocumentId} uploaded successfully for doctor {DoctorId}",
                    document.Id, currentDoctorId);

                var response = ApiResponse<DoctorDocumentResponse>.Success(
                    document,
                    "Award/certificate uploaded successfully",
                    201
                );
                return CreatedAtAction(
                    nameof(GetDocumentById),
                    new { documentId = document.Id },
                    response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request on award upload for doctor {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on award upload for doctor {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading award/certificate for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while uploading the award/certificate",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("me/documents/research")]
        [Authorize(Roles = "Doctor")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<DoctorDocumentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorDocumentResponse>>> UploadResearchPaper([FromForm] UploadDoctorDocumentRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to upload research paper - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Upload research paper request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for research paper upload. Errors: {Errors}",
                    string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var document = await _doctorService.UploadOrUpdateResearchPaperAsync(currentDoctorId, request);
                _logger.LogInformation("Research paper {DocumentId} uploaded successfully for doctor {DoctorId}",
                    document.Id, currentDoctorId);

                var response = ApiResponse<DoctorDocumentResponse>.Success(
                    document,
                    "Research paper uploaded successfully",
                    201
                );
                return CreatedAtAction(
                    nameof(GetDocumentById),
                    new { documentId = document.Id },
                    response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request on research paper upload for doctor {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on research paper upload for doctor {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading research paper for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while uploading the research paper",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Dashboard Operations

        [HttpGet("me/dashboard/stats")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<DoctorDashboardStatsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorDashboardStatsResponse>>> GetDashboardStats()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access dashboard stats - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

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
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
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

        /// <summary>
        /// جلب جميع مواعيد الدكتور المسجل مع إمكانية التصفية والترتيب
        /// </summary>
        [HttpGet("me/appointments")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DoctorAppointmentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DoctorAppointmentResponse>>>> GetMyAppointments(
            [FromQuery] GetDoctorAppointmentsRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access appointments - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

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

        [HttpGet("me/appointments/today")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<TodayAppointmentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<TodayAppointmentResponse>>>> GetTodayAppointments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access today's appointments - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

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
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
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

        #endregion

        #region Public Doctor Directory

        /// <summary>
        /// الحصول على قائمة الدكاترة مع pagination - معلومات مختصرة للعرض في القائمة
        /// </summary>
        [HttpGet("list")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DoctorListItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DoctorListItemResponse>>>> GetDoctorsList(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 6)
        {
            _logger.LogInformation("Request to get doctors list. Page: {Page}, Size: {Size}", pageNumber, pageSize);

            try
            {
                var paginationParams = new PaginationParams
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _doctorService.GetDoctorsListAsync(paginationParams);

                _logger.LogInformation("Successfully retrieved {Count} doctors out of {Total}", 
                    result.Data.Count(), result.TotalCount);

                return Ok(ApiResponse<PaginatedResponse<DoctorListItemResponse>>.Success(
                    result,
                    "Doctors list retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctors list");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving doctors list",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// الحصول على التفاصيل الكاملة للدكتور مع معلومات العيادة
        /// </summary>
        [HttpGet("{doctorId}/details")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<DoctorDetailsWithClinicResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorDetailsWithClinicResponse>>> GetDoctorDetailsWithClinic(Guid doctorId)
        {
            _logger.LogInformation("Request to get doctor details for doctor: {DoctorId}", doctorId);

            try
            {
                var result = await _doctorService.GetDoctorDetailsWithClinicAsync(doctorId);

                if (result == null)
                {
                    _logger.LogWarning("Doctor not found: {DoctorId}", doctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Doctor with ID {doctorId} not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Successfully retrieved doctor details for doctor: {DoctorId}", doctorId);

                return Ok(ApiResponse<DoctorDetailsWithClinicResponse>.Success(
                    result,
                    "Doctor details retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor details for doctor: {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving doctor details",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Doctor Patient Management Operations
        /// <summary>
        /// الحصول على السجل الطبي الكامل لمريض معين
        /// </summary>
        [HttpGet("me/patients/{patientId}/medical-record")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<PatientMedicalRecordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PatientMedicalRecordResponse>>> GetPatientMedicalRecord(Guid patientId)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to get patient medical record - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get medical record request for patient: {PatientId} by doctor: {DoctorId}", 
                patientId, currentDoctorId);

            try
            {
                var medicalRecord = await _doctorService.GetPatientMedicalRecordAsync(patientId, currentDoctorId);
                
                if (medicalRecord == null)
                {
                    _logger.LogWarning("Medical record not found for patient: {PatientId} or doctor has no access", patientId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Patient not found or you don't have access to this patient's medical record",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Successfully retrieved medical record for patient: {PatientId}", patientId);
                return Ok(ApiResponse<PatientMedicalRecordResponse>.Success(
                    medicalRecord,
                    "Medical record retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical record for patient: {PatientId}", patientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving medical record",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// الحصول على توثيق جميع الجلسات لمريض معين مع الدكتور
        /// </summary>
        [HttpGet("me/patients/{patientId}/session-documentations")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<PatientSessionDocumentationListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PatientSessionDocumentationListResponse>>> GetPatientSessionDocumentations(Guid patientId)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to get session documentations - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get session documentations request for patient: {PatientId} by doctor: {DoctorId}", 
                patientId, currentDoctorId);

            try
            {
                var sessionDocumentations = await _doctorService.GetPatientSessionDocumentationsAsync(patientId, currentDoctorId);
                
                if (sessionDocumentations == null)
                {
                    _logger.LogWarning("Session documentations not found for patient: {PatientId} or doctor has no sessions with patient", patientId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Patient not found or no completed sessions found with this patient",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Successfully retrieved {Count} session documentations for patient: {PatientId}", 
                    sessionDocumentations.Sessions.Count, patientId);
                
                return Ok(ApiResponse<PatientSessionDocumentationListResponse>.Success(
                    sessionDocumentations,
                    "Session documentations retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session documentations for patient: {PatientId}", patientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving session documentations",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// الحصول على جميع الروشتات لمريض معين من الدكتور
        /// </summary>
        [HttpGet("me/patients/{patientId}/prescriptions")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<PatientPrescriptionsListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PatientPrescriptionsListResponse>>> GetPatientPrescriptions(Guid patientId)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to get patient prescriptions - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get prescriptions request for patient: {PatientId} by doctor: {DoctorId}", 
                patientId, currentDoctorId);

            try
            {
                var prescriptions = await _doctorService.GetPatientPrescriptionsAsync(patientId, currentDoctorId);
                
                if (prescriptions == null)
                {
                    _logger.LogWarning("Prescriptions not found for patient: {PatientId} from doctor: {DoctorId}", 
                        patientId, currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Patient not found or no prescriptions found for this patient",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Successfully retrieved {Count} prescriptions for patient: {PatientId}", 
                    prescriptions.TotalPrescriptions, patientId);
                
                return Ok(ApiResponse<PatientPrescriptionsListResponse>.Success(
                    prescriptions,
                    "Prescriptions retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescriptions for patient: {PatientId}", patientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving prescriptions",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Booking System - Frontend Integration

        /// <summary>
        /// 1️⃣ GET Doctor's Weekly Schedule - جلب الجدول الأسبوعي للدكتور
        /// </summary>
        [HttpGet("{doctorId:guid}/appointments/schedule")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<DayScheduleSlotResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<DayScheduleSlotResponse>>>> GetDoctorWeeklySchedule(Guid doctorId)
        {
            _logger.LogInformation("Getting weekly schedule for doctor {DoctorId}", doctorId);

            try
            {
                var schedule = await _scheduleService.GetWeeklyScheduleForFrontendAsync(doctorId);
                return Ok(ApiResponse<List<DayScheduleSlotResponse>>.Success(
                    schedule,
                    "تم جلب الجدول الأسبوعي بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor {DoctorId} not found", doctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weekly schedule for doctor {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب الجدول الأسبوعي",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// 2️⃣ GET Doctor's Exceptional Dates - جلب المواعيد الاستثنائية
        /// </summary>
        [HttpGet("{doctorId:guid}/appointments/exceptions")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<ExceptionalDateResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<ExceptionalDateResponse>>>> GetDoctorExceptionalDates(Guid doctorId)
        {
            _logger.LogInformation("Getting exceptional dates for doctor {DoctorId}", doctorId);

            try
            {
                var exceptions = await _scheduleService.GetExceptionalDatesForFrontendAsync(doctorId);
                return Ok(ApiResponse<List<ExceptionalDateResponse>>.Success(
                    exceptions,
                    "تم جلب المواعيد الاستثنائية بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor {DoctorId} not found", doctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exceptional dates for doctor {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب المواعيد الاستثنائية",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// 3️⃣ GET Doctor's Services & Pricing - جلب أنواع الكشف والأسعار والمدة
        /// </summary>
        [HttpGet("{doctorId:guid}/services")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<DoctorServicesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorServicesResponse>>> GetDoctorServices(Guid doctorId)
        {
            _logger.LogInformation("Getting services for doctor {DoctorId}", doctorId);

            try
            {
                var services = await _servicePricingService.GetAllServicesAsync(doctorId);
                return Ok(ApiResponse<DoctorServicesResponse>.Success(
                    services,
                    "تم جلب الخدمات بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor {DoctorId} not found", doctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services for doctor {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب الخدمات",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// 4️⃣ GET Booked Appointments for Specific Date - جلب المواعيد المحجوزة ليوم معين
        /// </summary>
        [HttpGet("{doctorId:guid}/appointments/booked")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BookedAppointmentSlotResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookedAppointmentSlotResponse>>>> GetBookedAppointments(
            Guid doctorId,
            [FromQuery] string date)
        {
            _logger.LogInformation("Getting booked appointments for doctor {DoctorId} on date {Date}", doctorId, date);

            try
            {
                // Validate and parse date
                if (string.IsNullOrWhiteSpace(date))
                {
                    return BadRequest(ApiResponse<object>.Failure(
                        "التاريخ مطلوب",
                        new[] { "Date parameter is required" },
                        400
                    ));
                }

                if (!DateTime.TryParse(date, out var appointmentDate))
                {
                    return BadRequest(ApiResponse<object>.Failure(
                        "صيغة التاريخ غير صحيحة. الصيغة المطلوبة: YYYY-MM-DD",
                        new[] { "Invalid date format. Expected YYYY-MM-DD" },
                        400
                    ));
                }

                var bookedAppointments = await _appointmentService.GetBookedAppointmentsForDateAsync(doctorId, appointmentDate);
                return Ok(ApiResponse<IEnumerable<BookedAppointmentSlotResponse>>.Success(
                    bookedAppointments,
                    "تم جلب المواعيد المحجوزة بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor {DoctorId} not found", doctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booked appointments for doctor {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب المواعيد المحجوزة",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// 6️⃣ GET Available Time Slots (Optional) - حساب الفترات المتاحة
        /// </summary>
        [HttpGet("{doctorId:guid}/appointments/available-slots")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AvailableTimeSlotResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AvailableTimeSlotResponse>>>> GetAvailableTimeSlots(
            Guid doctorId,
            [FromQuery] string date,
            [FromQuery] int consultationType)
        {
            _logger.LogInformation("Getting available time slots for doctor {DoctorId} on date {Date} for consultationType {ConsultationType}",
                doctorId, date, consultationType);

            try
            {
                // Validate date
                if (string.IsNullOrWhiteSpace(date))
                {
                    return BadRequest(ApiResponse<object>.Failure(
                        "التاريخ مطلوب",
                        new[] { "Date parameter is required" },
                        400
                    ));
                }

                if (!DateTime.TryParse(date, out var appointmentDate))
                {
                    return BadRequest(ApiResponse<object>.Failure(
                        "صيغة التاريخ غير صحيحة. الصيغة المطلوبة: YYYY-MM-DD",
                        new[] { "Invalid date format. Expected YYYY-MM-DD" },
                        400
                    ));
                }

                // Validate consultationType
                if (consultationType != 0 && consultationType != 1)
                {
                    return BadRequest(ApiResponse<object>.Failure(
                        "نوع الاستشارة غير صحيح. يجب أن يكون 0 أو 1",
                        new[] { "Invalid consultationType. Must be 0 or 1" },
                        400
                    ));
                }

                var availableSlots = await _appointmentService.GetAvailableTimeSlotsAsync(doctorId, appointmentDate, consultationType);
                return Ok(ApiResponse<IEnumerable<AvailableTimeSlotResponse>>.Success(
                    availableSlots,
                    "تم حساب الفترات المتاحة بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor {DoctorId} not found", doctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available time slots for doctor {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء حساب الفترات المتاحة",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Doctor Patients Management

        /// <summary>
        /// جلب قائمة المرضى الذين لديهم على الأقل جلسة مكتملة مع الدكتور
        /// </summary>
        [HttpGet("me/patients")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DoctorPatientResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DoctorPatientResponse>>>> GetMyPatients(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access patients - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation(
                "Get patients request for doctor: {DoctorId}. Page: {Page}, Size: {Size}",
                currentDoctorId, pageNumber, pageSize);

            try
            {
                var paginationParams = new PaginationParams
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var patients = await _doctorService.GetDoctorPatientsWithPaginationAsync(
                    currentDoctorId, 
                    paginationParams);

                _logger.LogInformation(
                    "Patients retrieved successfully for doctor: {DoctorId}. Count: {Count}, TotalCount: {TotalCount}",
                    currentDoctorId, patients.Data.Count(), patients.TotalCount);

                return Ok(ApiResponse<PaginatedResponse<DoctorPatientResponse>>.Success(
                    patients,
                    "Patients retrieved successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving patients",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Utilities

        [HttpGet("specialty/all")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SpecialtyResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<SpecialtyResponse>>>> GetAllSpecialty()
        {
            _logger.LogInformation("Request received to get all specialties");

            try
            {
                var result = _doctorService.GetSpecialties();

                if (result == null || !result.Any())
                {
                    _logger.LogWarning("No specialties found");
                    return Ok(ApiResponse<IEnumerable<SpecialtyResponse>>.Success(result, "No specialties found"));
                }

                _logger.LogInformation("Retrieved {Count} specialties successfully", result.Count());
                return Ok(ApiResponse<IEnumerable<SpecialtyResponse>>.Success(result, "Specialties retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving specialties");
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving specialities", new[] { ex.Message }, 500));
            }
        }

        #endregion

        #region Session Management

        /// <summary>
        /// الحصول على الجلسة النشطة الحالية للدكتور
        /// GET /api/Doctors/me/active-session
        /// </summary>
        [HttpGet("me/active-session")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<Shuryan.Application.DTOs.Responses.Session.SessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Shuryan.Application.DTOs.Responses.Session.SessionResponse>>> GetMyActiveSession()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid authentication token", statusCode: 401));
            }

            try
            {
                _logger.LogInformation("Getting active session for Doctor {DoctorId}", currentDoctorId);

                var activeSession = await _sessionService.GetDoctorCurrentActiveSessionAsync(currentDoctorId);

                if (activeSession == null)
                {
                    _logger.LogInformation("No active session found for Doctor {DoctorId}", currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure("لا توجد جلسة نشطة حالياً", new[] { "لا توجد جلسة نشطة حالياً" }, 404));
                }

                _logger.LogInformation("Active session found for Doctor {DoctorId} with Appointment {AppointmentId}", 
                    currentDoctorId, activeSession.AppointmentId);

                return Ok(ApiResponse<Shuryan.Application.DTOs.Responses.Session.SessionResponse>.Success(
                    activeSession, 
                    "تم جلب الجلسة النشطة بنجاح"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active session for Doctor {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ غير متوقع", new[] { ex.Message }, 500));
            }
        }

        #endregion
    }
}
