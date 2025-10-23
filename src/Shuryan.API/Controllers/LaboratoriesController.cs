using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
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

        // ==================== CRUD Operations ====================

        /// <summary>
        /// Get all laboratories with optional filters
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LaboratoryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LaboratoryResponse>>> GetAllLaboratories(
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? offersHomeSampleCollection = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var laboratories = await _laboratoryService.GetAllLaboratoriesAsync(
                    searchTerm,
                    offersHomeSampleCollection,
                    includeInactive);
                return Ok(laboratories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all laboratories");
                return StatusCode(500, new { Message = "An error occurred while retrieving laboratories" });
            }
        }

        /// <summary>
        /// Get laboratory by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LaboratoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LaboratoryResponse>> GetLaboratory(Guid id)
        {
            try
            {
                var laboratory = await _laboratoryService.GetLaboratoryByIdAsync(id);
                if (laboratory == null)
                    return NotFound(new { Message = $"Laboratory with ID {id} not found" });

                return Ok(laboratory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting laboratory {LaboratoryId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get laboratory basic info
        /// </summary>
        [HttpGet("{id}/basic")]
        [ProducesResponseType(typeof(LaboratoryBasicResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LaboratoryBasicResponse>> GetLaboratoryBasicInfo(Guid id)
        {
            try
            {
                var laboratory = await _laboratoryService.GetLaboratoryBasicInfoAsync(id);
                if (laboratory == null)
                    return NotFound(new { Message = $"Laboratory with ID {id} not found" });

                return Ok(laboratory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting laboratory basic info {LaboratoryId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Create a new laboratory
        /// </summary>
        [HttpPost]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(LaboratoryResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LaboratoryResponse>> CreateLaboratory(
            [FromBody] CreateLaboratoryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var laboratory = await _laboratoryService.CreateLaboratoryAsync(request);
                return CreatedAtAction(
                    nameof(GetLaboratory),
                    new { id = laboratory.Id },
                    laboratory);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating laboratory");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating laboratory: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { 
                    Message = "An error occurred while creating the laboratory",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Update laboratory
        /// </summary>
        [HttpPut("{id}")]
        //[Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(typeof(LaboratoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LaboratoryResponse>> UpdateLaboratory(
            Guid id,
            [FromBody] UpdateLaboratoryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var laboratory = await _laboratoryService.UpdateLaboratoryAsync(id, request);
                return Ok(laboratory);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating laboratory {LaboratoryId}", id);
                return StatusCode(500, new { Message = "An error occurred while updating the laboratory" });
            }
        }

        /// <summary>
        /// Delete laboratory (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteLaboratory(Guid id)
        {
            try
            {
                var result = await _laboratoryService.DeleteLaboratoryAsync(id);
                if (!result)
                    return NotFound(new { Message = $"Laboratory with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting laboratory {LaboratoryId}", id);
                return StatusCode(500, new { Message = "An error occurred while deleting the laboratory" });
            }
        }

        // ==================== Lab Services Management ====================

        /// <summary>
        /// Get all services offered by a laboratory
        /// </summary>
        [HttpGet("{id}/services")]
        [ProducesResponseType(typeof(IEnumerable<LabServiceResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LabServiceResponse>>> GetLaboratoryServices(Guid id)
        {
            try
            {
                var services = await _laboratoryService.GetLaboratoryServicesAsync(id);
                return Ok(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services for laboratory {LaboratoryId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Add a new service to laboratory
        /// </summary>
        [HttpPost("{id}/services")]
        //[Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(typeof(LabServiceResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LabServiceResponse>> AddLaboratoryService(
            Guid id,
            [FromBody] CreateLabServiceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var service = await _laboratoryService.AddLaboratoryServiceAsync(id, request);
                return CreatedAtAction(
                    nameof(GetLaboratoryServices),
                    new { id },
                    service);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding service to laboratory {LaboratoryId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Update laboratory service
        /// </summary>
        [HttpPut("services/{serviceId}")]
        //[Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(typeof(LabServiceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LabServiceResponse>> UpdateLaboratoryService(
            Guid serviceId,
            [FromBody] CreateLabServiceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var service = await _laboratoryService.UpdateLaboratoryServiceAsync(serviceId, request);
                return Ok(service);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lab service {ServiceId}", serviceId);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Remove service from laboratory
        /// </summary>
        [HttpDelete("services/{serviceId}")]
        //[Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemoveLaboratoryService(Guid serviceId)
        {
            try
            {
                var result = await _laboratoryService.RemoveLaboratoryServiceAsync(serviceId);
                if (!result)
                    return NotFound(new { Message = $"Lab service with ID {serviceId} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing lab service {ServiceId}", serviceId);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        // ==================== Working Hours ====================

        /// <summary>
        /// Get laboratory working hours
        /// </summary>
        [HttpGet("{id}/working-hours")]
        [ProducesResponseType(typeof(IEnumerable<LabWorkingHoursResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LabWorkingHoursResponse>>> GetLaboratoryWorkingHours(Guid id)
        {
            try
            {
                var workingHours = await _laboratoryService.GetLaboratoryWorkingHoursAsync(id);
                return Ok(workingHours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting working hours for laboratory {LaboratoryId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Set laboratory working hours
        /// </summary>
        [HttpPost("{id}/working-hours")]
        //[Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> SetLaboratoryWorkingHours(
            Guid id,
            [FromBody] IEnumerable<CreateLabWorkingHoursRequest> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _laboratoryService.SetLaboratoryWorkingHoursAsync(id, request);
                return Ok(new { Message = "Working hours set successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting working hours for laboratory {LaboratoryId}", id);
                return StatusCode(500, new { 
                    Message = "An error occurred while setting working hours",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }

        // ==================== Search & Filter ====================

        /// <summary>
        /// Get laboratories offering specific test
        /// </summary>
        [HttpGet("offering-test/{labTestId}")]
        [ProducesResponseType(typeof(IEnumerable<LaboratoryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LaboratoryResponse>>> GetLaboratoriesOfferingTest(Guid labTestId)
        {
            try
            {
                var laboratories = await _laboratoryService.GetLaboratoriesOfferingTestAsync(labTestId);
                return Ok(laboratories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting laboratories offering test {LabTestId}", labTestId);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        // ==================== Statistics ====================

        /// <summary>
        /// Get laboratory statistics
        /// </summary>
        [HttpGet("{id}/statistics")]
        //[Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(typeof(LaboratoryStatistics), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LaboratoryStatistics>> GetLaboratoryStatistics(Guid id)
        {
            try
            {
                var statistics = await _laboratoryService.GetLaboratoryStatisticsAsync(id);
                return Ok(statistics);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for laboratory {LaboratoryId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }
    }
}
