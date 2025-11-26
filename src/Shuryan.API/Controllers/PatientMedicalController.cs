using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Application.DTOs.Responses.Prescription;
using Shuryan.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/patients/me")]
    [Authorize(Roles = "Patient")]
    public class PatientMedicalController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IPrescriptionService _prescriptionService;
        private readonly ILogger<PatientMedicalController> _logger;

        public PatientMedicalController(
            IPatientService patientService,
            IPrescriptionService prescriptionService,
            ILogger<PatientMedicalController> logger)
        {
            _patientService = patientService;
            _prescriptionService = prescriptionService;
            _logger = logger;
        }

        #region Medical History

        [HttpGet("medical-history")]
        [ProducesResponseType(typeof(IEnumerable<MedicalHistoryItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<MedicalHistoryItemResponse>>> GetMyMedicalHistory()
        {
            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Get medical history request for patient: {PatientId}", currentPatientId);

            try
            {
                var medicalHistory = await _patientService.GetPatientMedicalHistoryAsync(currentPatientId);
                _logger.LogInformation("Retrieved {Count} medical history items for patient: {PatientId}", medicalHistory.Count(), currentPatientId);
                return Ok(medicalHistory);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for medical history: {PatientId}", currentPatientId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical history for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving medical history" });
            }
        }

        [HttpPost("medical-history")]
        [ProducesResponseType(typeof(MedicalHistoryItemResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MedicalHistoryItemResponse>> AddMedicalHistoryItem(
            [FromBody] CreateMedicalHistoryItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid add medical history request");
                return BadRequest(ModelState);
            }

            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Add medical history item request for patient: {PatientId}", currentPatientId);

            try
            {
                var item = await _patientService.AddMedicalHistoryItemAsync(currentPatientId, request);
                _logger.LogInformation("Medical history item added successfully for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, item.Id);
                return CreatedAtAction(
                    nameof(GetMyMedicalHistory),
                    null,
                    item);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for adding medical history: {PatientId}", currentPatientId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding medical history item for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while adding medical history item" });
            }
        }

        [HttpPut("medical-history/{itemId}")]
        [ProducesResponseType(typeof(MedicalHistoryItemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MedicalHistoryItemResponse>> UpdateMedicalHistoryItem(
            Guid itemId,
            [FromBody] UpdateMedicalHistoryItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update medical history request for ItemId: {ItemId}", itemId);
                return BadRequest(ModelState);
            }

            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Update medical history item request for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);

            try
            {
                var item = await _patientService.UpdateMedicalHistoryItemAsync(currentPatientId, itemId, request);
                _logger.LogInformation("Medical history item updated successfully for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);
                return Ok(item);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Medical history item not found for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medical history item for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);
                return StatusCode(500, new { Message = "An unexpected error occurred while updating medical history item" });
            }
        }

        [HttpDelete("medical-history/{itemId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteMedicalHistoryItem(Guid itemId)
        {
            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Delete medical history item request for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);

            try
            {
                var result = await _patientService.DeleteMedicalHistoryItemAsync(currentPatientId, itemId);
                if (!result)
                {
                    _logger.LogWarning("Medical history item not found for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);
                    return NotFound(new { Message = "Medical history item not found" });
                }

                _logger.LogInformation("Medical history item deleted successfully for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting medical history item for patient: {PatientId}, ItemId: {ItemId}", currentPatientId, itemId);
                return StatusCode(500, new { Message = "An unexpected error occurred while deleting medical history item" });
            }
        }

        #endregion

        #region Prescriptions

        [HttpGet("prescriptions")]
        [ProducesResponseType(typeof(IEnumerable<PrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PrescriptionResponse>>> GetMyPrescriptions()
        {
            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Get prescriptions request for patient: {PatientId}", currentPatientId);

            try
            {
                var prescriptions = await _patientService.GetPatientPrescriptionsAsync(currentPatientId);
                _logger.LogInformation("Retrieved {Count} prescriptions for patient: {PatientId}", prescriptions.Count(), currentPatientId);
                return Ok(prescriptions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for prescriptions: {PatientId}", currentPatientId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescriptions for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving prescriptions" });
            }
        }

        [HttpGet("prescriptions/active")]
        [ProducesResponseType(typeof(IEnumerable<PrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PrescriptionResponse>>> GetMyActivePrescriptions()
        {
            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Get active prescriptions request for patient: {PatientId}", currentPatientId);

            try
            {
                var prescriptions = await _patientService.GetActivePrescriptionsAsync(currentPatientId);
                _logger.LogInformation("Retrieved {Count} active prescriptions for patient: {PatientId}", prescriptions.Count(), currentPatientId);
                return Ok(prescriptions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for active prescriptions: {PatientId}", currentPatientId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active prescriptions for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving active prescriptions" });
            }
        }

        [HttpGet("prescriptions/{prescriptionId}")]
        [ProducesResponseType(typeof(PrescriptionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PrescriptionResponse>> GetPrescriptionById(Guid prescriptionId)
        {
            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Get prescription by ID request for patient: {PatientId}, PrescriptionId: {PrescriptionId}", currentPatientId, prescriptionId);

            try
            {
                var prescription = await _patientService.GetPrescriptionByIdAsync(currentPatientId, prescriptionId);
                if (prescription == null)
                {
                    _logger.LogWarning("Prescription not found: {PrescriptionId} for patient: {PatientId}", prescriptionId, currentPatientId);
                    return NotFound(new { Message = $"Prescription with ID {prescriptionId} not found" });
                }

                _logger.LogInformation("Prescription retrieved successfully: {PrescriptionId} for patient: {PatientId}", prescriptionId, currentPatientId);
                return Ok(prescription);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for prescription: {PatientId}", currentPatientId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescription {PrescriptionId} for patient: {PatientId}", prescriptionId, currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving prescription" });
            }
        }

        /// <summary>
        /// Get all prescriptions for patient profile page with doctor info
        /// </summary>
        [HttpGet("prescriptions/list")]
        [ProducesResponseType(typeof(IEnumerable<PatientPrescriptionListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PatientPrescriptionListResponse>>> GetMyPrescriptionsList()
        {
            var currentPatientId = GetCurrentPatientId();

            _logger.LogInformation("Get prescriptions list for patient profile: {PatientId}", currentPatientId);

            try
            {
                var prescriptions = await _prescriptionService.GetPatientPrescriptionsListAsync(currentPatientId);

                _logger.LogInformation(
                    "Retrieved {Count} prescriptions for patient profile: {PatientId}",
                    prescriptions.Count(), currentPatientId);

                return Ok(prescriptions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for prescriptions list: {PatientId}", currentPatientId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescriptions list for patient: {PatientId}", currentPatientId);
                return StatusCode(500, new { Message = "An unexpected error occurred while retrieving prescriptions list" });
            }
        }

        #endregion

        // Note: Lab Orders endpoints moved to PatientLabController

        #region Helper Methods

        private Guid GetCurrentPatientId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        #endregion
    }
}
