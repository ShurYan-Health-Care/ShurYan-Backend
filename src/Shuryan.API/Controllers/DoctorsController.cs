using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Common.Pagination;
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

        public DoctorsController(
            IDoctorService doctorService,
            ILogger<DoctorsController> logger)
        {
            _doctorService = doctorService;
            _logger = logger;
        }

        #region Profile Operations - GET

        [HttpGet("me")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<DoctorProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DoctorProfileResponse>>> UpdateDoctorProfile(
            Guid id,
            [FromBody] UpdateDoctorProfileRequest request)
        {
            _logger.LogInformation("Update doctor profile request for doctor: {DoctorId}", id);

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
                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
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
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DoctorProfileResponse>>> UpdatePersonalInfo(
            [FromBody] UpdatePersonalInfoRequest request)
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
                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
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
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DoctorSpecialtyExperienceResponse>>> UpdateSpecialtyExperience(
            [FromBody] UpdateSpecialtyExperienceRequest request)
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
                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
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
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DoctorProfileResponse>>> UpdateMyProfileImage(
            [FromForm] UpdateProfileImageRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();
            _logger.LogInformation("Update profile image request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UpdateProfileImage for doctor: {DoctorId}. Errors: {Errors}",
                    currentDoctorId, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
            }

            try
            {
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
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
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

        #region Public Doctor Directory

        [HttpGet("list")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DoctorListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DoctorListItemResponse>>>> GetDoctorsList(
            [FromQuery] SearchDoctorsRequest searchRequest)
        {
            _logger.LogInformation("Request to get doctors list with filters. Page: {Page}, Size: {Size}, SearchTerm: {SearchTerm}, Specialty: {Specialty}, Governorate: {Governorate}, City: {City}, MinRating: {MinRating}, MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, AvailableToday: {AvailableToday}",
                searchRequest.PageNumber, searchRequest.PageSize, searchRequest.SearchTerm,
                searchRequest.Specialty ?? searchRequest.MedicalSpecialty, searchRequest.Governorate,
                searchRequest.City, searchRequest.MinRating, searchRequest.MinPrice, searchRequest.MaxPrice, searchRequest.AvailableToday);

            try
            {
                var result = await _doctorService.GetDoctorsListAsync(searchRequest);

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

        [HttpGet("{doctorId}/details")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<DoctorDetailsWithClinicResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
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

        #region Verification Operations

        [HttpPost("me/submit-for-review")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> SubmitForReview()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to submit for review - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Doctor {DoctorId} submitting profile for review", currentDoctorId);

            try
            {
                var result = await _doctorService.SubmitForReviewAsync(currentDoctorId);

                _logger.LogInformation("Doctor {DoctorId} successfully submitted profile for review", currentDoctorId);
                return Ok(ApiResponse<object>.Success(
                    new { submitted = result },
                    "Your profile has been submitted for review successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found for submit for review: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for submit for review: {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting profile for review for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while submitting your profile for review",
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
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving specialities",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Helper Methods

        private Guid GetCurrentDoctorId()
        {
            // Try multiple claim types that might contain the user ID
            var userIdClaim = User.Claims.FirstOrDefault(c => 
                c.Type == "sub" || 
                c.Type == "uid" || 
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            _logger.LogWarning("Could not find doctor ID in JWT claims. Available claims: {Claims}", 
                string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
            
            return Guid.Empty;
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
    }
}
