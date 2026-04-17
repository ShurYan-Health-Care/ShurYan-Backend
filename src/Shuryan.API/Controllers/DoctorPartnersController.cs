using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Common.Pagination;
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
    [Route("api/Doctors/me/partner")]
    [Authorize(Roles = "Doctor")]
    public class DoctorPartnersController : ControllerBase
    {
        private readonly IDoctorPartnerService _partnerService;
        private readonly ILogger<DoctorPartnersController> _logger;

        public DoctorPartnersController(IDoctorPartnerService partnerService, ILogger<DoctorPartnersController> logger)
        {
            _partnerService = partnerService;
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

        #region Current Suggested Partner
        [HttpGet("suggested")]
        [ProducesResponseType(typeof(ApiResponse<SuggestedPartnerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<SuggestedPartnerResponse>>> GetSuggestedPartner()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access suggested partner - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get suggested partner request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var partner = await _partnerService.GetSuggestedPartnerAsync(currentDoctorId);
                _logger.LogInformation("Suggested partner retrieved successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<SuggestedPartnerResponse>.Success(
                    partner,
                    "Suggested partner retrieved successfully"
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
                _logger.LogError(ex, "Error retrieving suggested partner for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving suggested partner",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("suggest")]
        [ProducesResponseType(typeof(ApiResponse<SuggestedPartnerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<SuggestedPartnerResponse>>> SuggestPartner([FromBody] SuggestPartnerRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to suggest partner - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Suggest partner request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for SuggestPartner for doctor: {DoctorId}. Errors: {Errors}",
                    currentDoctorId, string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    errors,
                    400
                ));
            }

            try
            {
                var partner = await _partnerService.SuggestPartnerAsync(currentDoctorId, request);
                _logger.LogInformation("Partner suggested successfully for doctor: {DoctorId}", currentDoctorId);

                // تحديد الرسالة بناءً على ما تم إضافته
                string message;
                if (request.PharmacyId.HasValue && request.LaboratoryId.HasValue)
                    message = "تم اقتراح الصيدلية والمعمل بنجاح";
                else if (request.PharmacyId.HasValue)
                    message = "تم اقتراح الصيدلية بنجاح";
                else
                    message = "تم اقتراح المعمل بنجاح";

                return Ok(ApiResponse<SuggestedPartnerResponse>.Success(
                    partner,
                    message
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for partner suggestion: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for partner suggestion: {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suggesting partner for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while suggesting partner",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpDelete("suggested")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> RemoveSuggestedPartner()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to remove suggested partner - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Remove suggested partner request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var result = await _partnerService.RemoveSuggestedPartnerAsync(currentDoctorId);
                if (!result)
                {
                    _logger.LogWarning("No suggested partner found for doctor: {DoctorId}", currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        "No suggested partner found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Suggested partner removed successfully for doctor: {DoctorId}", currentDoctorId);
                return Ok(ApiResponse<object>.Success(
                    null,
                    "تم إزالة الشريك المقترح بنجاح"
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
                _logger.LogError(ex, "Error removing suggested partner for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while removing suggested partner",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Available Partners
        [HttpGet("pharmacies")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<PartnerResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PartnerResponse>>>> GetAvailablePharmacies(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access available pharmacies - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get available pharmacies request for doctor: {DoctorId}. Page: {Page}, Size: {Size}",
                currentDoctorId, pageNumber, pageSize);

            try
            {
                var paginationParams = new PaginationParams
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var pharmacies = await _partnerService.GetAvailablePharmaciesAsync(paginationParams);
                _logger.LogInformation("Available pharmacies retrieved successfully. Count: {Count} of {Total}",
                    pharmacies.Data.Count(), pharmacies.TotalCount);

                return Ok(ApiResponse<PaginatedResponse<PartnerResponse>>.Success(
                    pharmacies,
                    "Available pharmacies retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available pharmacies");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving pharmacies",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("laboratories")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<PartnerResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PartnerResponse>>>> GetAvailableLaboratories(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access available laboratories - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get available laboratories request for doctor: {DoctorId}. Page: {Page}, Size: {Size}",
                currentDoctorId, pageNumber, pageSize);

            try
            {
                var paginationParams = new PaginationParams
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var laboratories = await _partnerService.GetAvailableLaboratoriesAsync(paginationParams);
                _logger.LogInformation("Available laboratories retrieved successfully. Count: {Count} of {Total}",
                    laboratories.Data.Count(), laboratories.TotalCount);

                return Ok(ApiResponse<PaginatedResponse<PartnerResponse>>.Success(
                    laboratories,
                    "Available laboratories retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available laboratories");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving laboratories",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion
    }
}
