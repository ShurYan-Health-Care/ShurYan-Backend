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

        #region Profile Management
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorProfileResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DoctorProfileResponse>>>> GetAllDoctors(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Get all doctors request. PageNumber: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);
            
            try
            {
                var doctors = await _doctorService.GetAllDoctorsAsync(pageNumber, pageSize);
                _logger.LogInformation("Successfully retrieved {Count} doctors", doctors.Count());
                return Ok(ApiResponse<IEnumerable<DoctorProfileResponse>>.Success(
                    doctors, 
                    "Doctors retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all doctors");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving doctors",
                    new[] { ex.Message },
                    500
                ));
            }
        }

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

        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorProfileResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DoctorProfileResponse>>>> SearchDoctors(
            [FromQuery] string searchTerm,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Search doctors request. SearchTerm: {SearchTerm}, PageNumber: {PageNumber}, PageSize: {PageSize}", 
                searchTerm, pageNumber, pageSize);
            
            try
            {
                var doctors = await _doctorService.SearchDoctorsAsync(searchTerm, pageNumber, pageSize);
                _logger.LogInformation("Successfully found {Count} doctors for search term: {SearchTerm}", doctors.Count(), searchTerm);
                return Ok(ApiResponse<IEnumerable<DoctorProfileResponse>>.Success(
                    doctors, 
                    "Doctor search successful"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching doctors with term: {SearchTerm}", searchTerm);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while searching doctors",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("specialty/{specialty}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorProfileResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DoctorProfileResponse>>>> GetDoctorsBySpecialty(
            string specialty,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Getting doctors by specialty: {Specialty}, PageNumber: {PageNumber}, PageSize: {PageSize}", specialty, pageNumber, pageSize);
            try
            {
                var doctors = await _doctorService.GetDoctorsBySpecialtyAsync(specialty, pageNumber, pageSize);
                return Ok(ApiResponse<IEnumerable<DoctorProfileResponse>>.Success(doctors, "Doctors retrieved successfully by specialty"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctors by specialty: {Specialty}", specialty);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving doctors by specialty", new[] { ex.Message }, 500));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        [ProducesResponseType(typeof(ApiResponse<DoctorProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorProfileResponse>>> UpdateDoctorProfile(
            Guid id,
            [FromBody] UpdateDoctorProfileRequest request)
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

        [HttpGet("{id}/statistics")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<DoctorStatisticsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorStatisticsResponse>>> GetDoctorStatistics(Guid id)
        {
            _logger.LogInformation("Getting statistics for doctor: {DoctorId}", id);
            try
            {
                var statistics = await _doctorService.GetDoctorStatisticsAsync(id);
                _logger.LogInformation("Statistics retrieved successfully for doctor: {DoctorId}", id);
                return Ok(ApiResponse<DoctorStatisticsResponse>.Success(statistics, "Statistics retrieved successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found when getting statistics: {DoctorId}", id);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctor statistics for doctor: {DoctorId}", id);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while getting statistics", new[] { ex.Message }, 500));
            }
        }

        [HttpPut("{id}/profile-image")]
        [Authorize(Roles = "Doctor,Admin")]
        [ProducesResponseType(typeof(ApiResponse<DoctorProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorProfileResponse>>> UpdateProfileImage(
            Guid id,
            [FromBody] UpdateProfileImageRequest request)
        {
            _logger.LogInformation("Update profile image request for doctor: {DoctorId}", id);

            // Ownership check: Doctors can only update their own profile image unless they're Admin
            if (!IsAdmin() && !IsAccessingOwnData(id))
            {
                _logger.LogWarning("Forbidden: Doctor {CurrentDoctorId} attempted to update profile image of another doctor {DoctorId}", 
                    GetCurrentDoctorId(), id);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure(
                    "Doctors can only update their own profile image",
                    statusCode: 403
                ));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UpdateProfileImage for doctor: {DoctorId}. Errors: {Errors}", 
                    id, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var doctor = await _doctorService.UpdateProfileImageAsync(id, request.ImageUrl);
                _logger.LogInformation("Profile image updated successfully for doctor: {DoctorId}", id);
                return Ok(ApiResponse<DoctorProfileResponse>.Success(
                    doctor,
                    "Profile image updated successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found for profile image update: {DoctorId}", id);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile image for doctor: {DoctorId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating profile image",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("top-rated")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorProfileResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DoctorProfileResponse>>>> GetTopRatedDoctors(
            [FromQuery] int count = 10)
        {
            _logger.LogInformation("Getting top {Count} rated doctors", count);
            try
            {
                var doctors = await _doctorService.GetTopRatedDoctorsAsync(count);
                return Ok(ApiResponse<IEnumerable<DoctorProfileResponse>>.Success(doctors, "Top rated doctors retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top-rated doctors");
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while getting top-rated doctors", new[] { ex.Message }, 500));
            }
        }

        [HttpGet("by-governorate/{governorate}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorProfileResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DoctorProfileResponse>>>> GetDoctorsByGovernorate(
            string governorate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Getting doctors by governorate: {Governorate}, PageNumber: {PageNumber}, PageSize: {PageSize}", governorate, pageNumber, pageSize);
            try
            {
                var doctors = await _doctorService.GetDoctorsByGovernorateAsync(governorate, pageNumber, pageSize);
                return Ok(ApiResponse<IEnumerable<DoctorProfileResponse>>.Success(doctors, "Doctors retrieved successfully by governorate"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctors by governorate: {Governorate}", governorate);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while getting doctors by governorate", new[] { ex.Message }, 500));
            }
        }
        #endregion

        #region Document Management
        [HttpGet("{doctorId}/documents")]
        [Authorize(Roles = "Doctor,Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorDocumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DoctorDocumentResponse>>>> GetDoctorDocuments(Guid doctorId)
        {
            _logger.LogInformation("Get doctor documents request for doctor: {DoctorId}", doctorId);

            // Ownership check: Doctors can only view their own documents unless they're Admin
            if (!IsAdmin() && !IsAccessingOwnData(doctorId))
            {
                _logger.LogWarning("Forbidden: Doctor {CurrentDoctorId} attempted to get documents of another doctor {DoctorId}", 
                    GetCurrentDoctorId(), doctorId);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure(
                    "Doctors can only view their own documents",
                    statusCode: 403
                ));
            }

            try
            {
                var documents = await _doctorService.GetDoctorDocumentsAsync(doctorId);
                _logger.LogInformation("Successfully retrieved {Count} documents for doctor: {DoctorId}", documents.Count(), doctorId);
                return Ok(ApiResponse<IEnumerable<DoctorDocumentResponse>>.Success(
                    documents,
                    "Documents retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor documents for doctor: {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving doctor documents",
                    new[] { ex.Message },
                    500
                ));
            }
        }

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

        [HttpPost("{doctorId}/documents")]
        [Authorize(Roles = "Doctor,Admin")]
        [ProducesResponseType(typeof(ApiResponse<DoctorDocumentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorDocumentResponse>>> UploadDocument(
            Guid doctorId,
            [FromBody] UploadDoctorDocumentRequest request)
        {
            _logger.LogInformation("Upload document request for doctor: {DoctorId}", doctorId);

            // Ownership check: Doctors can only upload documents for their own profile unless they're Admin
            if (!IsAdmin() && !IsAccessingOwnData(doctorId))
            {
                _logger.LogWarning("Forbidden: Doctor {CurrentDoctorId} attempted to upload document for another doctor {DoctorId}", 
                    GetCurrentDoctorId(), doctorId);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure(
                    "Doctors can only upload documents for their own profile",
                    statusCode: 403
                ));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UploadDocument for doctor: {DoctorId}. Errors: {Errors}", 
                    doctorId, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var document = await _doctorService.UploadDocumentAsync(doctorId, request);
                _logger.LogInformation("Document {DocumentId} uploaded successfully for doctor {DoctorId}", document.Id, doctorId);

                var response = ApiResponse<DoctorDocumentResponse>.Success(
                    document,
                    "Document uploaded successfully",
                    201
                );
                return CreatedAtAction(
                    nameof(GetDocumentById),
                    new { documentId = document.Id },
                    response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request on document upload for doctor {DoctorId}", doctorId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on document upload for doctor {DoctorId}", doctorId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for doctor: {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while uploading the document",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("documents/{documentId}")]
        [Authorize(Roles = "Doctor,Admin")]
        [ProducesResponseType(typeof(ApiResponse<DoctorDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorDocumentResponse>>> UpdateDocument(Guid documentId, [FromBody] UploadDoctorDocumentRequest request)
        {
            _logger.LogInformation("Attempting to update document: {DocumentId}", documentId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UpdateDocument: {DocumentId}. Errors: {Errors}", documentId, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
            }

            try
            {
                var document = await _doctorService.UpdateDocumentAsync(documentId, request);
                _logger.LogInformation("Document updated successfully: {DocumentId}", documentId);
                return Ok(ApiResponse<DoctorDocumentResponse>.Success(document, "Document updated successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Document not found on update: {DocumentId}", documentId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document: {DocumentId}", documentId);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while updating the document", new[] { ex.Message }, 500));
            }
        }

        [HttpPut("documents/{documentId}/submit")]
        [Authorize(Roles = "Doctor,Admin")]
        [ProducesResponseType(typeof(ApiResponse<DoctorDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorDocumentResponse>>> SubmitDocumentForReview(Guid documentId)
        {
            _logger.LogInformation("Attempting to submit document for review: {DocumentId}", documentId);

            try
            {
                var document = await _doctorService.SubmitDocumentForReviewAsync(documentId);
                _logger.LogInformation("Document {DocumentId} submitted for review successfully", documentId);
                return Ok(ApiResponse<DoctorDocumentResponse>.Success(document, "Document submitted for review"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Document not found on submission: {DocumentId}", documentId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on document submission: {DocumentId}", documentId);
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting document for review: {DocumentId}", documentId);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while submitting the document", new[] { ex.Message }, 500));
            }
        }

        [HttpDelete("documents/{documentId}")]
        [Authorize(Roles = "Doctor,Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteDocument(Guid documentId)
        {
            _logger.LogInformation("Attempting to delete document: {DocumentId}", documentId);

            try
            {
                var result = await _doctorService.DeleteDocumentAsync(documentId);
                if (!result)
                {
                    _logger.LogWarning("Document not found for deletion: {DocumentId}", documentId);
                    return NotFound(ApiResponse<object>.Failure($"Document with ID {documentId} not found", statusCode: 404));
                }

                _logger.LogInformation("Document deleted successfully: {DocumentId}", documentId);
                return Ok(ApiResponse<object>.Success(null, "Document deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document: {DocumentId}", documentId);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while deleting the document", new[] { ex.Message }, 500));
            }
        }
        #endregion
    }
}

