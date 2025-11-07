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
    /// <summary>
    /// Patients Administration Controller
    /// Handles admin-level patient management operations (CRUD, Search, Query)
    /// </summary>
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
    }
}
