using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Clinic;
using Shuryan.Application.DTOs.Responses.Clinic;
using Shuryan.Application.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/Doctors/me/clinic")]
    [Authorize(Roles = "Doctor")]
    public class ClinicsController : ControllerBase
    {
        private readonly IClinicService _clinicService;
        private readonly ILogger<ClinicsController> _logger;

        public ClinicsController(IClinicService clinicService, ILogger<ClinicsController> logger)
        {
            _clinicService = clinicService;
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
        #endregion

        #region Clinic Info
        [HttpGet("info")]
        [ProducesResponseType(typeof(ApiResponse<ClinicInfoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ClinicInfoResponse>>> GetClinicInfo()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access clinic info - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get clinic info request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var clinicInfo = await _clinicService.GetClinicInfoAsync(currentDoctorId);
                if (clinicInfo == null)
                {
                    _logger.LogWarning("Clinic info not found for doctor: {DoctorId}", currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Clinic information not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Clinic info retrieved successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<ClinicInfoResponse>.Success(
                    clinicInfo,
                    "Clinic information retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clinic info for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving clinic information",
                    new[] { ex.Message },
                    500
                ));
            }
        }


        [HttpPut("info")]
        [ProducesResponseType(typeof(ApiResponse<ClinicInfoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ClinicInfoResponse>>> UpdateClinicInfo([FromBody] UpdateClinicInfoRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update clinic info - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Update clinic info request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UpdateClinicInfo for doctor: {DoctorId}. Errors: {Errors}",
                    currentDoctorId, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var clinicInfo = await _clinicService.UpdateClinicInfoAsync(currentDoctorId, request);
                _logger.LogInformation("Clinic info updated successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<ClinicInfoResponse>.Success(
                    clinicInfo,
                    "تم تحديث معلومات العيادة بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found for clinic info update: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating clinic info for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating clinic information",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Clinic Address
        [HttpGet("address")]
        [ProducesResponseType(typeof(ApiResponse<ClinicAddressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ClinicAddressResponse>>> GetClinicAddress()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access clinic address - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get clinic address request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var address = await _clinicService.GetClinicAddressAsync(currentDoctorId);
                if (address == null)
                {
                    _logger.LogWarning("Clinic address not found for doctor: {DoctorId}", currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Clinic address not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Clinic address retrieved successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<ClinicAddressResponse>.Success(
                    address,
                    "Clinic address retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clinic address for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving clinic address",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("address")]
        [ProducesResponseType(typeof(ApiResponse<ClinicAddressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ClinicAddressResponse>>> UpdateClinicAddress([FromBody] UpdateClinicAddressRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update clinic address - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Update clinic address request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UpdateClinicAddress for doctor: {DoctorId}. Errors: {Errors}",
                    currentDoctorId, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var address = await _clinicService.UpdateClinicAddressAsync(currentDoctorId, request);
                _logger.LogInformation("Clinic address updated successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<ClinicAddressResponse>.Success(
                    address,
                    "تم تحديث عنوان العيادة بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found for clinic address update: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for clinic address update: {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating clinic address for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating clinic address",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Clinic Images
        [HttpGet("images")]
        [ProducesResponseType(typeof(ApiResponse<ClinicImagesListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ClinicImagesListResponse>>> GetClinicImages()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access clinic images - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get clinic images request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var images = await _clinicService.GetClinicImagesAsync(currentDoctorId);
                _logger.LogInformation("Clinic images retrieved successfully for doctor: {DoctorId}. Count: {Count}",
                    currentDoctorId, images.Images.Count);
                return Ok(ApiResponse<ClinicImagesListResponse>.Success(
                    images,
                    "Clinic images retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clinic images for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving clinic images",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("images")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<ClinicImageResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ClinicImageResponse>>> UploadClinicImage([FromForm] UploadClinicImageRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to upload clinic image - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Upload clinic image request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UploadClinicImage for doctor: {DoctorId}. Errors: {Errors}",
                    currentDoctorId, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var image = await _clinicService.UploadClinicImageAsync(currentDoctorId, request);
                _logger.LogInformation("Clinic image uploaded successfully for doctor: {DoctorId}. Image ID: {ImageId}",
                    currentDoctorId, image.Id);
                return CreatedAtAction(
                    nameof(GetClinicImages),
                    new { },
                    ApiResponse<ClinicImageResponse>.Success(
                        image,
                        "تم رفع الصورة بنجاح"
                    )
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for clinic image upload: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for clinic image upload: {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading clinic image for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while uploading clinic image",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpDelete("images/{imageId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteClinicImage(Guid imageId)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to delete clinic image - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Delete clinic image request for doctor: {DoctorId}. Image ID: {ImageId}",
                currentDoctorId, imageId);

            try
            {
                var result = await _clinicService.DeleteClinicImageAsync(currentDoctorId, imageId);
                if (!result)
                {
                    _logger.LogWarning("Failed to delete clinic image {ImageId} for doctor: {DoctorId}", imageId, currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Image not found or could not be deleted",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Clinic image deleted successfully for doctor: {DoctorId}. Image ID: {ImageId}",
                    currentDoctorId, imageId);
                return Ok(ApiResponse<object>.Success(
                    null,
                    "تم حذف الصورة بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for clinic image deletion: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for clinic image deletion: {DoctorId}", currentDoctorId);
                return StatusCode(403, ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 403
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting clinic image for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while deleting clinic image",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion
    }
}
