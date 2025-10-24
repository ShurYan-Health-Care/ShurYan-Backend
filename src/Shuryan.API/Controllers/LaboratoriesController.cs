using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
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
    [Authorize] // Secure the controller by default
    public class LaboratoriesController : ControllerBase
    {
        private readonly ILaboratoryService _laboratoryService;
        private readonly ILogger<LaboratoriesController> _logger;

        public LaboratoriesController(
            ILaboratoryService laboratoryService,
            ILogger<LaboratoriesController> logger)
        {
            _laboratoryService = laboratoryService;
            _logger = logger;
        }

        #region Helper Methods

        /// <summary>
        /// Gets the current authenticated user's ID.
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userIdClaim) ? Guid.Empty : Guid.Parse(userIdClaim);
        }

        /// <summary>
        /// Checks if the current user is an Admin.
        /// </summary>
        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Get all laboratories with optional filters (Public)
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // This is a public search endpoint
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LaboratoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<LaboratoryResponse>>>> GetAllLaboratories(
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? offersHomeSampleCollection = null,
            [FromQuery] bool includeInactive = false)
        {
            _logger.LogInformation("Get all laboratories request with filters: SearchTerm={SearchTerm}, OffersHomeSampleCollection={OffersHomeSampleCollection}, IncludeInactive={IncludeInactive}",
                searchTerm, offersHomeSampleCollection, includeInactive);

            try
            {
                var laboratories = await _laboratoryService.GetAllLaboratoriesAsync(
                    searchTerm,
                    offersHomeSampleCollection,
                    includeInactive);

                _logger.LogInformation("Successfully retrieved {Count} laboratories", laboratories.Count());
                return Ok(ApiResponse<IEnumerable<LaboratoryResponse>>.Success(
                    laboratories,
                    "Laboratories retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all laboratories");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving laboratories",
                    new[] { ex.Message },
                    500));
            }
        }

        /// <summary>
        /// Get laboratory by ID (Public)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous] // Public endpoint
        [ProducesResponseType(typeof(ApiResponse<LaboratoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LaboratoryResponse>>> GetLaboratory(Guid id)
        {
            _logger.LogInformation("Get laboratory request for laboratory: {LaboratoryId}", id);

            try
            {
                var laboratory = await _laboratoryService.GetLaboratoryByIdAsync(id);
                if (laboratory == null)
                {
                    _logger.LogWarning("Laboratory not found for laboratory: {LaboratoryId}", id);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Laboratory with ID {id} not found",
                        statusCode: 404));
                }

                _logger.LogInformation("Laboratory retrieved successfully for laboratory: {LaboratoryId}", id);
                return Ok(ApiResponse<LaboratoryResponse>.Success(
                    laboratory,
                    "Laboratory retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting laboratory for laboratory: {LaboratoryId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving the laboratory",
                    new[] { ex.Message },
                    500));
            }
        }

        /// <summary>
        /// Get laboratory basic info (Public)
        /// </summary>
        [HttpGet("{id}/basic")]
        [AllowAnonymous] // Public endpoint
        [ProducesResponseType(typeof(ApiResponse<LaboratoryBasicResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LaboratoryBasicResponse>>> GetLaboratoryBasicInfo(Guid id)
        {
            _logger.LogInformation("Get laboratory basic info request for laboratory: {LaboratoryId}", id);

            try
            {
                var laboratory = await _laboratoryService.GetLaboratoryBasicInfoAsync(id);
                if (laboratory == null)
                {
                    _logger.LogWarning("Laboratory basic info not found for laboratory: {LaboratoryId}", id);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Laboratory with ID {id} not found",
                        statusCode: 404));
                }

                _logger.LogInformation("Laboratory basic info retrieved successfully for laboratory: {LaboratoryId}", id);
                return Ok(ApiResponse<LaboratoryBasicResponse>.Success(
                    laboratory,
                    "Laboratory basic info retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting laboratory basic info for laboratory: {LaboratoryId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving the laboratory basic info",
                    new[] { ex.Message },
                    500));
            }
        }

        /// <summary>
        /// Create a new laboratory (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<LaboratoryResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LaboratoryResponse>>> CreateLaboratory(
            [FromBody] CreateLaboratoryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid model state",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray(),
                    400));

            _logger.LogInformation("Create laboratory request by Admin {AdminId}", GetCurrentUserId());

            try
            {
                var laboratory = await _laboratoryService.CreateLaboratoryAsync(request);

                _logger.LogInformation("Laboratory created successfully with ID: {LaboratoryId}", laboratory.Id);

                var response = ApiResponse<LaboratoryResponse>.Success(
                    laboratory,
                    "Laboratory created successfully", 201);

                return CreatedAtAction(
                    nameof(GetLaboratory),
                    new { id = laboratory.Id },
                    response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating laboratory");
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating laboratory: {ErrorMessage}", ex.Message);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while creating the laboratory",
                    new[] { ex.Message },
                    500));
            }
        }

        /// <summary>
        /// Update laboratory (Laboratory owner or Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(typeof(ApiResponse<LaboratoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LaboratoryResponse>>> UpdateLaboratory(
            Guid id,
            [FromBody] UpdateLaboratoryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid model state",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray(),
                    400));

            _logger.LogInformation("Update laboratory request for laboratory: {LaboratoryId}", id);

            // Security check: Labs can only update their own profile
            var currentUserId = GetCurrentUserId();
            if (User.IsInRole("Laboratory") && !IsAdmin() && currentUserId != id)
            {
                _logger.LogWarning("Forbidden: Laboratory {CurrentUserId} attempted to update another laboratory {LaboratoryId}", currentUserId, id);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("Laboratories can only update their own profile", statusCode: 403));
            }

            try
            {
                var laboratory = await _laboratoryService.UpdateLaboratoryAsync(id, request);

                _logger.LogInformation("Laboratory updated successfully for laboratory: {LaboratoryId}", id);
                return Ok(ApiResponse<LaboratoryResponse>.Success(
                    laboratory,
                    "Laboratory updated successfully"));
            }
            catch (ArgumentException ex) // Not found
            {
                _logger.LogWarning("Laboratory not found for update: {LaboratoryId}", id);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating laboratory for laboratory: {LaboratoryId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while updating the laboratory",
                    new[] { ex.Message },
                    500));
            }
        }

        /// <summary>
        /// Delete laboratory (soft delete) (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteLaboratory(Guid id)
        {
            _logger.LogInformation("Delete laboratory request for laboratory: {LaboratoryId}", id);

            try
            {
                var result = await _laboratoryService.DeleteLaboratoryAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Laboratory not found for deletion: {LaboratoryId}", id);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Laboratory with ID {id} not found",
                        statusCode: 404));
                }

                _logger.LogInformation("Laboratory deleted successfully: {LaboratoryId}", id);
                return Ok(ApiResponse<object>.Success(
                    null,
                    "Laboratory deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting laboratory for laboratory: {LaboratoryId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while deleting the laboratory",
                    new[] { ex.Message },
                    500));
            }
        }

        #endregion

        #region Lab Services Management

        /// <summary>
        /// Get all services offered by a laboratory (Public)
        /// </summary>
        [HttpGet("{id}/services")]
        [AllowAnonymous] // Public endpoint
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabServiceResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<LabServiceResponse>>>> GetLaboratoryServices(Guid id)
        {
            _logger.LogInformation("Get laboratory services request for laboratory: {LaboratoryId}", id);

            try
            {
                var services = await _laboratoryService.GetLaboratoryServicesAsync(id);

                _logger.LogInformation("Successfully retrieved {Count} services for laboratory: {LaboratoryId}", services.Count(), id);
                return Ok(ApiResponse<IEnumerable<LabServiceResponse>>.Success(
                    services,
                    "Laboratory services retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services for laboratory: {LaboratoryId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving laboratory services",
                    new[] { ex.Message },
                    500));
            }
        }

        /// <summary>
        /// Add a new service to laboratory (Laboratory owner or Admin)
        /// </summary>
        [HttpPost("{id}/services")]
        [Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(typeof(ApiResponse<LabServiceResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LabServiceResponse>>> AddLaboratoryService(
            Guid id,
            [FromBody] CreateLabServiceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid model state",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray(),
                    400));

            _logger.LogInformation("Add laboratory service request for laboratory: {LaboratoryId}", id);

            // Security check: Labs can only add services to their own profile
            var currentUserId = GetCurrentUserId();
            if (User.IsInRole("Laboratory") && !IsAdmin() && currentUserId != id)
            {
                _logger.LogWarning("Forbidden: Laboratory {CurrentUserId} attempted to add service to another laboratory {LaboratoryId}", currentUserId, id);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("Laboratories can only add services to their own profile", statusCode: 403));
            }

            try
            {
                var service = await _laboratoryService.AddLaboratoryServiceAsync(id, request);

                _logger.LogInformation("Laboratory service added successfully for laboratory: {LaboratoryId}", id);

                var response = ApiResponse<LabServiceResponse>.Success(
                        service,
                        "Laboratory service added successfully", 201);

                return CreatedAtAction(
                    nameof(GetLaboratoryServices), // TODO: Should be GetServiceById
                    new { id },
                    response);
            }
            catch (ArgumentException ex) // e.g., Lab not found, or test not found
            {
                _logger.LogWarning(ex, "Invalid argument while adding service to laboratory: {LaboratoryId}", id);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding service to laboratory: {LaboratoryId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while adding the laboratory service",
                    new[] { ex.Message },
                    500));
            }
        }

        /// <summary>
        /// Update laboratory service (Laboratory owner or Admin)
        /// </summary>
        [HttpPut("services/{serviceId}")]
        [Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(typeof(ApiResponse<LabServiceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LabServiceResponse>>> UpdateLaboratoryService(
            Guid serviceId,
            [FromBody] CreateLabServiceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid model state",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray(),
                    400));

            _logger.LogInformation("Update laboratory service request for service: {ServiceId}", serviceId);

            // TODO: Security check should be in the service layer
            // The service must verify that the authenticated user (GetCurrentUserId())
            // is either an Admin or the owner of the lab that this 'serviceId' belongs to.

            try
            {
                var service = await _laboratoryService.UpdateLaboratoryServiceAsync(serviceId, request);

                _logger.LogInformation("Laboratory service updated successfully for service: {ServiceId}", serviceId);
                return Ok(ApiResponse<LabServiceResponse>.Success(
                    service,
                    "Laboratory service updated successfully"));
            }
            catch (ArgumentException ex) // Not found
            {
                _logger.LogWarning("Laboratory service not found for update: {ServiceId}", serviceId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lab service: {ServiceId}", serviceId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while updating the laboratory service",
                    new[] { ex.Message },
                    500));
            }
        }

        /// <summary>
        /// Remove service from laboratory (Laboratory owner or Admin)
        /// </summary>
        [HttpDelete("services/{serviceId}")]
        [Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> RemoveLaboratoryService(Guid serviceId)
        {
            _logger.LogInformation("Remove laboratory service request for service: {ServiceId}", serviceId);

            // TODO: Security check should be in the service layer
            // The service must verify that the authenticated user (GetCurrentUserId())
            // is either an Admin or the owner of the lab that this 'serviceId' belongs to.

            try
            {
                var result = await _laboratoryService.RemoveLaboratoryServiceAsync(serviceId);
                if (!result)
                {
                    _logger.LogWarning("Lab service not found for removal: {ServiceId}", serviceId);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Lab service with ID {serviceId} not found",
                        statusCode: 404));
                }

                _logger.LogInformation("Laboratory service removed successfully: {ServiceId}", serviceId);
                return Ok(ApiResponse<object>.Success(
                    null,
                    "Laboratory service removed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing lab service: {ServiceId}", serviceId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while removing the laboratory service",
                    new[] { ex.Message },
                    500));
            }
        }

        #endregion

        #region Working Hours

        /// <summary>
        /// Get laboratory working hours (Public)
        /// </summary>
        [HttpGet("{id}/working-hours")]
        [AllowAnonymous] // Public endpoint
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabWorkingHoursResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<LabWorkingHoursResponse>>>> GetLaboratoryWorkingHours(Guid id)
        {
            _logger.LogInformation("Get laboratory working hours request for laboratory: {LaboratoryId}", id);

            try
            {
                var workingHours = await _laboratoryService.GetLaboratoryWorkingHoursAsync(id);

                _logger.LogInformation("Successfully retrieved working hours for laboratory: {LaboratoryId}", id);
                return Ok(ApiResponse<IEnumerable<LabWorkingHoursResponse>>.Success(
                    workingHours,
                    "Laboratory working hours retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting working hours for laboratory: {LaboratoryId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving laboratory working hours",
                    new[] { ex.Message },
                    500));
            }
        }

        /// <summary>
        /// Set laboratory working hours (Laboratory owner or Admin)
        /// </summary>
        [HttpPost("{id}/working-hours")]
        [Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> SetLaboratoryWorkingHours(
            Guid id,
            [FromBody] IEnumerable<CreateLabWorkingHoursRequest> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid model state",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray(),
                    400));

            _logger.LogInformation("Set laboratory working hours request for laboratory: {LaboratoryId}", id);

            // Security check: Labs can only set their own working hours
            var currentUserId = GetCurrentUserId();
            if (User.IsInRole("Laboratory") && !IsAdmin() && currentUserId != id)
            {
                _logger.LogWarning("Forbidden: Laboratory {CurrentUserId} attempted to set hours for another laboratory {LaboratoryId}", currentUserId, id);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("Laboratories can only set their own working hours", statusCode: 403));
            }

            try
            {
                await _laboratoryService.SetLaboratoryWorkingHoursAsync(id, request);

                _logger.LogInformation("Working hours set successfully for laboratory: {LaboratoryId}", id);
                return Ok(ApiResponse<object>.Success(
                    null,
                    "Working hours set successfully"));
            }
            catch (ArgumentException ex) // e.g., Lab not found
            {
                _logger.LogWarning(ex, "Invalid argument while setting working hours for laboratory: {LaboratoryId}", id);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting working hours for laboratory: {LaboratoryId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while setting working hours",
                    new[] { ex.Message },
                    500));
            }
        }

        #endregion

        #region Search & Filter

        /// <summary>
        /// Get laboratories offering specific test (Public)
        /// </summary>
        [HttpGet("offering-test/{labTestId}")]
        [AllowAnonymous] // Public endpoint
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LaboratoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<LaboratoryResponse>>>> GetLaboratoriesOfferingTest(Guid labTestId)
        {
            _logger.LogInformation("Get laboratories offering test request for test: {LabTestId}", labTestId);

            try
            {
                var laboratories = await _laboratoryService.GetLaboratoriesOfferingTestAsync(labTestId);

                _logger.LogInformation("Successfully retrieved {Count} laboratories offering test: {LabTestId}", laboratories.Count(), labTestId);
                return Ok(ApiResponse<IEnumerable<LaboratoryResponse>>.Success(
                    laboratories,
                    "Laboratories offering test retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting laboratories offering test: {LabTestId}", labTestId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving laboratories offering test",
                    new[] { ex.Message },
                    500));
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Get laboratory statistics (Laboratory owner or Admin)
        /// </summary>
        [HttpGet("{id}/statistics")]
        [Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(typeof(ApiResponse<LaboratoryStatistics>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LaboratoryStatistics>>> GetLaboratoryStatistics(Guid id)
        {
            _logger.LogInformation("Get laboratory statistics request for laboratory: {LaboratoryId}", id);

            // Security check: Labs can only view their own stats
            var currentUserId = GetCurrentUserId();
            if (User.IsInRole("Laboratory") && !IsAdmin() && currentUserId != id)
            {
                _logger.LogWarning("Forbidden: Laboratory {CurrentUserId} attempted to get stats for another laboratory {LaboratoryId}", currentUserId, id);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("Laboratories can only view their own statistics", statusCode: 403));
            }

            try
            {
                var statistics = await _laboratoryService.GetLaboratoryStatisticsAsync(id);

                _logger.LogInformation("Laboratory statistics retrieved successfully for laboratory: {LaboratoryId}", id);
                return Ok(ApiResponse<LaboratoryStatistics>.Success(
                    statistics,
                    "Laboratory statistics retrieved successfully"));
            }
            catch (ArgumentException ex) // Not found
            {
                _logger.LogWarning("Laboratory not found for statistics: {LaboratoryId}", id);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for laboratory: {LaboratoryId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving laboratory statistics",
                    new[] { ex.Message },
                    500));
            }
        }

        #endregion
    }
}
