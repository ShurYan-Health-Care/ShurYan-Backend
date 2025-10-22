using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.DTOs.Requests.Prescription;
using Shuryan.Application.DTOs.Responses.Prescription;
using Shuryan.Application.Interfaces;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // كل الـ endpoints محتاجة authentication
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly ILogger<PrescriptionsController> _logger;

        public PrescriptionsController(
            IPrescriptionService prescriptionService,
            ILogger<PrescriptionsController> logger)
        {
            _prescriptionService = prescriptionService;
            _logger = logger;
        }

        // ==================== CORE CRUD ====================

        [HttpGet]
        [ProducesResponseType(typeof(PrescriptionQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PrescriptionQueryResponse>> GetPrescriptions(
            [FromQuery] PrescriptionQueryParams queryParams)
        {
            // Validation
            if (!queryParams.IsValid(out string validationError))
                return BadRequest(new { Message = validationError });

            try
            {
                var result = await _prescriptionService.GetPrescriptionsAsync(queryParams);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions with filters: {@Filters}", queryParams);
                return StatusCode(500, new { Message = "An error occurred while retrieving prescriptions" });
            }
        }

        /// <summary>
        /// Get prescription by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PrescriptionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PrescriptionResponse>> GetPrescription(Guid id)
        {
            try
            {
                var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id);
                if (prescription == null)
                    return NotFound(new { Message = $"Prescription with ID {id} not found" });

                return Ok(prescription);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get prescription by prescription number
        /// </summary>
        [HttpGet("number/{prescriptionNumber}")]
        [ProducesResponseType(typeof(PrescriptionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PrescriptionResponse>> GetPrescriptionByNumber(string prescriptionNumber)
        {
            try
            {
                var prescription = await _prescriptionService.GetPrescriptionByNumberAsync(prescriptionNumber);
                if (prescription == null)
                    return NotFound(new { Message = $"Prescription with number {prescriptionNumber} not found" });

                return Ok(prescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription by number {PrescriptionNumber}", prescriptionNumber);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Create a new prescription
        /// </summary>
        [HttpPost]
        //[Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(PrescriptionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PrescriptionResponse>> CreatePrescription(
            [FromBody] CreatePrescriptionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var prescription = await _prescriptionService.CreatePrescriptionAsync(request);
                return CreatedAtAction(
                    nameof(GetPrescription),
                    new { id = prescription.Id },
                    prescription);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription");
                return StatusCode(500, new { 
                    Message = "An error occurred while creating the prescription",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Update prescription
        /// </summary>
        [HttpPut("{id}")]
        //[Authorize(Roles = "Doctor")] // Disabled for testing
        [ProducesResponseType(typeof(PrescriptionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PrescriptionResponse>> UpdatePrescription(
            Guid id,
            [FromBody] UpdatePrescriptionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var prescription = await _prescriptionService.UpdatePrescriptionAsync(id, request);
                return Ok(prescription);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred while updating the prescription" });
            }
        }

        /// <summary>
        /// Delete prescription (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Doctor,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePrescription(Guid id)
        {
            try
            {
                var result = await _prescriptionService.DeletePrescriptionAsync(id);
                if (!result)
                    return NotFound(new { Message = $"Prescription with ID {id} not found" });

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred while deleting the prescription" });
            }
        }

        // ==================== PRESCRIPTION LIFECYCLE ====================

        /// <summary>
        /// Cancel a prescription
        /// </summary>
        [HttpPost("{id}/cancel")]
        //[Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(PrescriptionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PrescriptionResponse>> CancelPrescription(
            Guid id,
            [FromBody] CancelPrescriptionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _prescriptionService.CancelPrescriptionAsync(id, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred while cancelling the prescription" });
            }
        }

        /// <summary>
        /// Renew an existing prescription
        /// </summary>
        [HttpPost("{id}/renew")]
        //[Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(PrescriptionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PrescriptionResponse>> RenewPrescription(
            Guid id,
            [FromBody] RenewPrescriptionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var prescription = await _prescriptionService.RenewPrescriptionAsync(id, request);
                return CreatedAtAction(
                    nameof(GetPrescription),
                    new { id = prescription.Id },
                    prescription);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing prescription {PrescriptionId}", id);
                return StatusCode(500, new { 
                    Message = "An error occurred while renewing the prescription",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }

        // ==================== PHARMACY OPERATIONS ====================

        /// <summary>
        /// Verify prescription authenticity (for pharmacies)
        /// </summary>
        [HttpGet("{id}/verify")]
        //[Authorize(Roles = "Pharmacy,Pharmacist")]
        [ProducesResponseType(typeof(PrescriptionVerificationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PrescriptionVerificationResponse>> VerifyPrescription(
            Guid id,
            [FromQuery] string? verificationCode = null)
        {
            try
            {
                var result = await _prescriptionService.VerifyPrescriptionAsync(id, verificationCode);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred while verifying the prescription" });
            }
        }

        /// <summary>
        /// Mark prescription as dispensed by pharmacy
        /// </summary>
        [HttpPost("{id}/dispense")]
        //[Authorize(Roles = "Pharmacy,Pharmacist")]
        [ProducesResponseType(typeof(DispenseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DispenseResult>> DispensePrescription(
            Guid id,
            [FromBody] DispensePrescriptionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _prescriptionService.DispensePrescriptionAsync(id, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispensing prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred while dispensing the prescription" });
            }
        }

        /// <summary>
        /// Mark prescription as digitally shared with pharmacy
        /// </summary>
        [HttpPost("{id}/share")]
        //[Authorize(Roles = "Doctor,Patient")]
        [ProducesResponseType(typeof(SharePrescriptionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SharePrescriptionResult>> SharePrescription(
            Guid id,
            [FromBody] SharePrescriptionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _prescriptionService.SharePrescriptionAsync(id, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharing prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get prescription dispensing history
        /// </summary>
        [HttpGet("{id}/dispensing-history")]
        //[Authorize(Roles = "Doctor,Pharmacy,Admin")]
        [ProducesResponseType(typeof(IEnumerable<DispensingRecord>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<DispensingRecord>>> GetDispensingHistory(Guid id)
        {
            try
            {
                var history = await _prescriptionService.GetDispensingHistoryAsync(id);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dispensing history for prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Accept prescription delivery to pharmacy
        /// </summary>
        [HttpPost("{id}/accept-delivery")]
        //[Authorize(Roles = "Pharmacy,Pharmacist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AcceptPrescriptionDelivery(
            Guid id,
            [FromBody] AcceptDeliveryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _prescriptionService.AcceptPrescriptionDeliveryAsync(id, request);
                
                // Get prescription details to return
                var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id);
                
                return Ok(new { 
                    Message = "Prescription accepted for delivery",
                    Success = true,
                    PrescriptionId = id,
                    PharmacyId = request.PharmacyId,
                    EstimatedDeliveryMinutes = request.EstimatedDeliveryMinutes,
                    DeliveryAddress = request.DeliveryAddress,
                    DeliveryFee = request.DeliveryFee,
                    DeliveryNotes = request.DeliveryNotes,
                    Prescription = prescription
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting delivery for prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        // ==================== PATIENT SPECIALIZED ENDPOINTS ====================

        /// <summary>
        /// Get patient's current active medications
        /// </summary>
        [HttpGet("patient/{patientId}/current-medications")]
        //[Authorize(Roles = "Patient,Doctor,Admin")]
        [ProducesResponseType(typeof(IEnumerable<CurrentMedicationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CurrentMedicationResponse>>> GetCurrentMedications(
            Guid patientId)
        {
            try
            {
                var medications = await _prescriptionService.GetCurrentMedicationsAsync(patientId);
                return Ok(medications);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current medications for patient {PatientId}", patientId);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        // ==================== ANALYTICS & STATISTICS ====================

        /// <summary>
        /// Get prescription status history (audit trail)
        /// </summary>
        [HttpGet("{id}/status-history")]
        //[Authorize(Roles = "Doctor,Admin")]
        [ProducesResponseType(typeof(IEnumerable<PrescriptionStatusHistory>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PrescriptionStatusHistory>>> GetStatusHistory(Guid id)
        {
            try
            {
                var history = await _prescriptionService.GetStatusHistoryAsync(id);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status history for prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }
    }
}