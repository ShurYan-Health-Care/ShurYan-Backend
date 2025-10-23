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
    public class LabPrescriptionsController : ControllerBase
    {
        private readonly ILabPrescriptionService _prescriptionService;
        private readonly ILogger<LabPrescriptionsController> _logger;

        public LabPrescriptionsController(
            ILabPrescriptionService prescriptionService,
            ILogger<LabPrescriptionsController> logger)
        {
            _prescriptionService = prescriptionService;
            _logger = logger;
        }

        /// <summary>
        /// Get all lab prescriptions with optional filters
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LabPrescriptionResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LabPrescriptionResponse>>> GetAllLabPrescriptions(
            [FromQuery] Guid? doctorId = null,
            [FromQuery] Guid? patientId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var prescriptions = await _prescriptionService.GetAllLabPrescriptionsAsync(
                    doctorId,
                    patientId,
                    startDate,
                    endDate);
                return Ok(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all lab prescriptions");
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get lab prescription by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LabPrescriptionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LabPrescriptionResponse>> GetLabPrescription(Guid id)
        {
            try
            {
                var prescription = await _prescriptionService.GetLabPrescriptionByIdAsync(id);
                if (prescription == null)
                    return NotFound(new { Message = $"Lab prescription with ID {id} not found" });

                return Ok(prescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get lab prescription by appointment ID
        /// </summary>
        [HttpGet("appointment/{appointmentId}")]
        [ProducesResponseType(typeof(LabPrescriptionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LabPrescriptionResponse>> GetLabPrescriptionByAppointment(Guid appointmentId)
        {
            try
            {
                var prescription = await _prescriptionService.GetLabPrescriptionByAppointmentIdAsync(appointmentId);
                if (prescription == null)
                    return NotFound(new { Message = $"Lab prescription for appointment {appointmentId} not found" });

                return Ok(prescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab prescription for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get patient's lab prescriptions
        /// </summary>
        [HttpGet("patient/{patientId}")]
        //[Authorize(Roles = "Patient,Doctor,Admin")]
        [ProducesResponseType(typeof(IEnumerable<LabPrescriptionResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LabPrescriptionResponse>>> GetPatientLabPrescriptions(Guid patientId)
        {
            try
            {
                var prescriptions = await _prescriptionService.GetPatientLabPrescriptionsAsync(patientId);
                return Ok(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab prescriptions for patient {PatientId}", patientId);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get doctor's lab prescriptions
        /// </summary>
        [HttpGet("doctor/{doctorId}")]
        //[Authorize(Roles = "Doctor,Admin")]
        [ProducesResponseType(typeof(IEnumerable<LabPrescriptionResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LabPrescriptionResponse>>> GetDoctorLabPrescriptions(Guid doctorId)
        {
            try
            {
                var prescriptions = await _prescriptionService.GetDoctorLabPrescriptionsAsync(doctorId);
                return Ok(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab prescriptions for doctor {DoctorId}", doctorId);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Create a new lab prescription
        /// </summary>
        [HttpPost]
        //[Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(LabPrescriptionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LabPrescriptionResponse>> CreateLabPrescription(
            [FromBody] CreateLabPrescriptionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var prescription = await _prescriptionService.CreateLabPrescriptionAsync(request);
                return CreatedAtAction(
                    nameof(GetLabPrescription),
                    new { id = prescription.Id },
                    prescription);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lab prescription");
                return StatusCode(500, new { 
                    Message = "An error occurred",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Update lab prescription
        /// </summary>
        [HttpPut("{id}")]
        //[Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(LabPrescriptionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LabPrescriptionResponse>> UpdateLabPrescription(
            Guid id,
            [FromBody] CreateLabPrescriptionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var prescription = await _prescriptionService.UpdateLabPrescriptionAsync(id, request);
                return Ok(prescription);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lab prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Delete lab prescription
        /// </summary>
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Doctor,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteLabPrescription(Guid id)
        {
            try
            {
                var result = await _prescriptionService.DeleteLabPrescriptionAsync(id);
                if (!result)
                    return NotFound(new { Message = $"Lab prescription with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lab prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        // ==================== Prescription Items ====================

        /// <summary>
        /// Get prescription items
        /// </summary>
        [HttpGet("{id}/items")]
        [ProducesResponseType(typeof(IEnumerable<LabPrescriptionItemResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LabPrescriptionItemResponse>>> GetPrescriptionItems(Guid id)
        {
            try
            {
                var items = await _prescriptionService.GetPrescriptionItemsAsync(id);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting items for prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Add item to prescription
        /// </summary>
        [HttpPost("{id}/items")]
        //[Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(LabPrescriptionItemResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LabPrescriptionItemResponse>> AddPrescriptionItem(
            Guid id,
            [FromBody] CreateLabPrescriptionItemRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var item = await _prescriptionService.AddPrescriptionItemAsync(id, request);
                return CreatedAtAction(
                    nameof(GetPrescriptionItems),
                    new { id },
                    item);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to prescription {PrescriptionId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Remove item from prescription
        /// </summary>
        [HttpDelete("items/{itemId}")]
        //[Authorize(Roles = "Doctor")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemovePrescriptionItem(Guid itemId)
        {
            try
            {
                var result = await _prescriptionService.RemovePrescriptionItemAsync(itemId);
                if (!result)
                    return NotFound(new { Message = $"Prescription item with ID {itemId} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing prescription item {ItemId}", itemId);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }
    }
}
