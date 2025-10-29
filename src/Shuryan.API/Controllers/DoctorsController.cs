using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Doctor;
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
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(IDoctorService doctorService, ILogger<DoctorsController> logger)
        {
            _doctorService = doctorService;
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
    }
}
