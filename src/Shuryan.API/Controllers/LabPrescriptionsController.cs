//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using Shuryan.Application.DTOs.Common.Base;
//using Shuryan.Application.DTOs.Requests.Laboratory;
//using Shuryan.Application.DTOs.Responses.Laboratory;
//using Shuryan.Application.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;

//namespace Shuryan.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize]
//    public class LabPrescriptionsController : ControllerBase
//    {
//        private readonly ILabPrescriptionService _prescriptionService;
//        private readonly ILogger<LabPrescriptionsController> _logger;

//        public LabPrescriptionsController(
//            ILabPrescriptionService prescriptionService,
//            ILogger<LabPrescriptionsController> logger)
//        {
//            _prescriptionService = prescriptionService;
//            _logger = logger;
//        }

//        #region Helper Methods
//        private Guid GetCurrentUserId()
//        {
//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
//            {
//                return Guid.Empty;
//            }
//            return userId;
//        }

//        private bool IsAccessingOwnData(Guid userId)
//        {
//            var currentUserId = GetCurrentUserId();
//            return currentUserId == userId;
//        }

//        private bool IsAdmin()
//        {
//            return User.IsInRole("Admin");
//        }
//        #endregion

//        #region CRUD Operations
//        [HttpGet]
//        [Authorize(Roles = "Admin")]
//        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabPrescriptionResponse>>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<IEnumerable<LabPrescriptionResponse>>>> GetAllLabPrescriptions(
//            [FromQuery] Guid? doctorId = null,
//            [FromQuery] Guid? patientId = null,
//            [FromQuery] DateTime? startDate = null,
//            [FromQuery] DateTime? endDate = null)
//        {
//            _logger.LogInformation("Admin {AdminId} getting all lab prescriptions with filters", GetCurrentUserId());
            
//            try
//            {
//                var prescriptions = await _prescriptionService.GetAllLabPrescriptionsAsync(
//                    doctorId,
//                    patientId,
//                    startDate,
//                    endDate);
                
//                _logger.LogInformation("Successfully retrieved {Count} lab prescriptions", prescriptions.Count());
//                return Ok(ApiResponse<IEnumerable<LabPrescriptionResponse>>.Success(
//                    prescriptions,
//                    "Prescriptions retrieved successfully"
//                ));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error retrieving all lab prescriptions");
//                return StatusCode(500, ApiResponse<object>.Failure(
//                    "An unexpected error occurred while retrieving prescriptions",
//                    new[] { ex.Message },
//                    500
//                ));
//            }
//        }

//        [HttpGet("{id}")]
//        [Authorize(Roles = "Patient,Doctor,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<LabPrescriptionResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabPrescriptionResponse>>> GetLabPrescription(Guid id)
//        {
//            _logger.LogInformation("Get lab prescription request for prescription: {PrescriptionId}", id);
            
//            try
//            {
//                var prescription = await _prescriptionService.GetLabPrescriptionByIdAsync(id);
//                if (prescription == null)
//                {
//                    _logger.LogWarning("Lab prescription not found: {PrescriptionId}", id);
//                    return NotFound(ApiResponse<object>.Failure(
//                        $"Lab prescription with ID {id} not found",
//                        statusCode: 404
//                    ));
//                }

//                // Security Check: Users can only access prescriptions they own unless they're Admin
//                var currentUserId = GetCurrentUserId();
//                if (!IsAdmin() && prescription.PatientId != currentUserId && prescription.DoctorId != currentUserId)
//                {
//                    _logger.LogWarning("Forbidden: User {CurrentUserId} attempted to access prescription {PrescriptionId} belonging to Patient {PatientId} and Doctor {DoctorId}",
//                        currentUserId, id, prescription.PatientId, prescription.DoctorId);
//                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure(
//                        "You are not authorized to view this prescription",
//                        statusCode: 403
//                    ));
//                }

//                _logger.LogInformation("Lab prescription retrieved successfully: {PrescriptionId}", id);
//                return Ok(ApiResponse<LabPrescriptionResponse>.Success(
//                    prescription,
//                    "Prescription retrieved successfully"
//                ));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error retrieving lab prescription {PrescriptionId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure(
//                    "An unexpected error occurred while retrieving the prescription",
//                    new[] { ex.Message },
//                    500
//                ));
//            }
//        }

//        [HttpPost]
//        [Authorize(Roles = "Doctor")]
//        [ProducesResponseType(typeof(ApiResponse<LabPrescriptionResponse>), StatusCodes.Status201Created)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabPrescriptionResponse>>> CreateLabPrescription(
//            [FromBody] CreateLabPrescriptionRequest request)
//        {
//            _logger.LogInformation("Create lab prescription request for patient {PatientId} by doctor {DoctorId}", 
//                request.PatientId, request.DoctorId);

//            // Security check: Doctor can only create prescriptions for themselves
//            var currentUserId = GetCurrentUserId();
//            if (currentUserId != request.DoctorId)
//            {
//                _logger.LogWarning("Forbidden: Doctor {CurrentUserId} attempted to create prescription as doctor {DoctorId}", 
//                    currentUserId, request.DoctorId);
//                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure(
//                    "Doctors can only create prescriptions as themselves",
//                    statusCode: 403
//                ));
//            }

//            if (!ModelState.IsValid)
//            {
//                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
//                _logger.LogWarning("Invalid model state for CreateLabPrescription. Errors: {Errors}", string.Join(", ", errors));
//                return BadRequest(ApiResponse<object>.Failure(
//                    "Invalid request data",
//                    errors,
//                    400
//                ));
//            }

//            try
//            {
//                var prescription = await _prescriptionService.CreateLabPrescriptionAsync(request);
//                _logger.LogInformation("Lab prescription {PrescriptionId} created successfully", prescription.Id);

//                var response = ApiResponse<LabPrescriptionResponse>.Success(
//                    prescription,
//                    "Prescription created successfully",
//                    201
//                );
//                return CreatedAtAction(
//                    nameof(GetLabPrescription),
//                    new { id = prescription.Id },
//                    response);
//            }
//            catch (ArgumentException ex)
//            {
//                _logger.LogWarning(ex, "Bad request on lab prescription creation");
//                return BadRequest(ApiResponse<object>.Failure(
//                    ex.Message,
//                    statusCode: 400
//                ));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error creating lab prescription");
//                return StatusCode(500, ApiResponse<object>.Failure(
//                    "An unexpected error occurred while creating the prescription",
//                    new[] { ex.Message },
//                    500
//                ));
//            }
//        }

//        /// <summary>
//        /// Update lab prescription (Doctor only - creator)
//        /// </summary>
//        [HttpPut("{id}")]
//        [Authorize(Roles = "Doctor")]
//        [ProducesResponseType(typeof(ApiResponse<LabPrescriptionResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabPrescriptionResponse>>> UpdateLabPrescription(
//            Guid id,
//            [FromBody] CreateLabPrescriptionRequest request)
//        {
//            _logger.LogInformation("Attempting to update lab prescription {PrescriptionId}", id);

//            if (!ModelState.IsValid)
//            {
//                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
//                _logger.LogWarning("Invalid model state for UpdateLabPrescription. Errors: {Errors}", string.Join(", ", errors));
//                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
//            }

//            // TODO: Service layer must check that GetCurrentUserId() matches the prescription's DoctorId

//            try
//            {
//                var prescription = await _prescriptionService.UpdateLabPrescriptionAsync(id, request);
//                _logger.LogInformation("Lab prescription {PrescriptionId} updated successfully", id);
//                return Ok(ApiResponse<LabPrescriptionResponse>.Success(prescription, "Prescription updated successfully"));
//            }
//            catch (ArgumentException ex) // Not found
//            {
//                _logger.LogWarning(ex, "Lab prescription not found for update: {PrescriptionId}", id);
//                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error updating lab prescription {PrescriptionId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while updating the prescription", new[] { ex.Message }, 500));
//            }
//        }

//        /// <summary>
//        /// Delete lab prescription (Doctor or Admin)
//        /// </summary>
//        [HttpDelete("{id}")]
//        [Authorize(Roles = "Doctor,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<object>>> DeleteLabPrescription(Guid id)
//        {
//            _logger.LogInformation("Attempting to delete lab prescription {PrescriptionId}", id);

//            // TODO: Service layer must check that GetCurrentUserId() matches the prescription's DoctorId OR user is Admin

//            try
//            {
//                var result = await _prescriptionService.DeleteLabPrescriptionAsync(id);
//                if (!result)
//                {
//                    _logger.LogWarning("Lab prescription not found for deletion: {PrescriptionId}", id);
//                    return NotFound(ApiResponse<object>.Failure($"Lab prescription with ID {id} not found", statusCode: 404));
//                }

//                _logger.LogInformation("Lab prescription {PrescriptionId} deleted successfully", id);
//                return Ok(ApiResponse<object>.Success(null, "Prescription deleted successfully"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error deleting lab prescription {PrescriptionId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while deleting the prescription", new[] { ex.Message }, 500));
//            }
//        }

//        #endregion

//        #region Prescription Lookups

//        /// <summary>
//        /// Get lab prescription by appointment ID. Accessible by owner (Patient, Doctor) or Admin.
//        /// </summary>
//        [HttpGet("appointment/{appointmentId}")]
//        [Authorize(Roles = "Patient,Doctor,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<LabPrescriptionResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabPrescriptionResponse>>> GetLabPrescriptionByAppointment(Guid appointmentId)
//        {
//            _logger.LogInformation("Attempting to get lab prescription for appointment {AppointmentId}", appointmentId);
//            try
//            {
//                var prescription = await _prescriptionService.GetLabPrescriptionByAppointmentIdAsync(appointmentId);
//                if (prescription == null)
//                {
//                    _logger.LogWarning("Lab prescription not found for appointment {AppointmentId}", appointmentId);
//                    return NotFound(ApiResponse<object>.Failure($"Lab prescription for appointment {appointmentId} not found", statusCode: 404));
//                }

//                // Security Check
//                var currentUserId = GetCurrentUserId();
//                if (!IsAdmin() && prescription.PatientId != currentUserId && prescription.DoctorId != currentUserId)
//                {
//                    _logger.LogWarning("Forbidden: User {CurrentUserId} attempted to access prescription for appointment {AppointmentId}", currentUserId, appointmentId);
//                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("You are not authorized to view this prescription", statusCode: 403));
//                }

//                return Ok(ApiResponse<LabPrescriptionResponse>.Success(prescription, "Prescription retrieved successfully"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting lab prescription for appointment {AppointmentId}", appointmentId);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving the prescription", new[] { ex.Message }, 500));
//            }
//        }

//        /// <summary>
//        /// Get patient's lab prescriptions. Accessible by the Patient, associated Doctor, or Admin.
//        /// </summary>
//        [HttpGet("patient/{patientId}")]
//        [Authorize(Roles = "Patient,Doctor,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabPrescriptionResponse>>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<IEnumerable<LabPrescriptionResponse>>>> GetPatientLabPrescriptions(Guid patientId)
//        {
//            _logger.LogInformation("Attempting to get lab prescriptions for patient {PatientId}", patientId);

//            var currentUserId = GetCurrentUserId();
//            // Patient can only see their own. Doctor/Admin can see any.
//            if (User.IsInRole("Patient") && !IsAdmin() && currentUserId != patientId)
//            {
//                _logger.LogWarning("Forbidden: Patient {CurrentUserId} attempted to access prescriptions of patient {PatientId}", currentUserId, patientId);
//                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("Patients can only view their own prescriptions", statusCode: 403));
//            }
//            // TODO: A Doctor should only be able to see their own patients' prescriptions. This check should be in the service layer.

//            try
//            {
//                var prescriptions = await _prescriptionService.GetPatientLabPrescriptionsAsync(patientId);
//                return Ok(ApiResponse<IEnumerable<LabPrescriptionResponse>>.Success(prescriptions, "Patient prescriptions retrieved successfully"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting lab prescriptions for patient {PatientId}", patientId);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving prescriptions", new[] { ex.Message }, 500));
//            }
//        }

//        /// <summary>
//        /// Get doctor's lab prescriptions. Accessible by the Doctor or Admin.
//        /// </summary>
//        [HttpGet("doctor/{doctorId}")]
//        [Authorize(Roles = "Doctor,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabPrescriptionResponse>>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<IEnumerable<LabPrescriptionResponse>>>> GetDoctorLabPrescriptions(Guid doctorId)
//        {
//            _logger.LogInformation("Attempting to get lab prescriptions for doctor {DoctorId}", doctorId);

//            var currentUserId = GetCurrentUserId();
//            if (User.IsInRole("Doctor") && !IsAdmin() && currentUserId != doctorId)
//            {
//                _logger.LogWarning("Forbidden: Doctor {CurrentUserId} attempted to access prescriptions of doctor {DoctorId}", currentUserId, doctorId);
//                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("Doctors can only view their own prescriptions", statusCode: 403));
//            }

//            try
//            {
//                var prescriptions = await _prescriptionService.GetDoctorLabPrescriptionsAsync(doctorId);
//                return Ok(ApiResponse<IEnumerable<LabPrescriptionResponse>>.Success(prescriptions, "Doctor prescriptions retrieved successfully"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting lab prescriptions for doctor {DoctorId}", doctorId);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving prescriptions", new[] { ex.Message }, 500));
//            }
//        }

//        #endregion

//        #region Prescription Items

//        /// <summary>
//        /// Get prescription items. Accessible by owner (Patient, Doctor) or Admin.
//        /// </summary>
//        [HttpGet("{id}/items")]
//        [Authorize(Roles = "Patient,Doctor,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabPrescriptionItemResponse>>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<IEnumerable<LabPrescriptionItemResponse>>>> GetPrescriptionItems(Guid id)
//        {
//            _logger.LogInformation("Attempting to get items for prescription {PrescriptionId}", id);

//            // TODO: Service layer must check that user has access to the parent prescription (id)

//            try
//            {
//                var items = await _prescriptionService.GetPrescriptionItemsAsync(id);
//                return Ok(ApiResponse<IEnumerable<LabPrescriptionItemResponse>>.Success(items, "Prescription items retrieved successfully"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting items for prescription {PrescriptionId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving items", new[] { ex.Message }, 500));
//            }
//        }

//        /// <summary>
//        /// Add item to prescription (Doctor only - creator)
//        /// </summary>
//        [HttpPost("{id}/items")]
//        [Authorize(Roles = "Doctor")]
//        [ProducesResponseType(typeof(ApiResponse<LabPrescriptionItemResponse>), StatusCodes.Status201Created)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabPrescriptionItemResponse>>> AddPrescriptionItem(
//            Guid id,
//            [FromBody] CreateLabPrescriptionItemRequest request)
//        {
//            _logger.LogInformation("Attempting to add item to prescription {PrescriptionId}", id);

//            if (!ModelState.IsValid)
//            {
//                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
//                _logger.LogWarning("Invalid model state for AddPrescriptionItem. Errors: {Errors}", string.Join(", ", errors));
//                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
//            }

//            // TODO: Service layer must check that GetCurrentUserId() matches the prescription's DoctorId

//            try
//            {
//                var item = await _prescriptionService.AddPrescriptionItemAsync(id, request);
//                _logger.LogInformation("Item with LabTestId {LabTestId} added to prescription {PrescriptionId}", item.LabTestId, id); // <-- This line was fixed

//                var response = ApiResponse<LabPrescriptionItemResponse>.Success(item, "Item added successfully", 201);
//                return CreatedAtAction(
//                    nameof(GetPrescriptionItems), // TODO: Should be GetItemById
//                    new { id },
//                    response);
//            }
//            catch (ArgumentException ex) // e.g., Prescription not found or Test not found
//            {
//                _logger.LogWarning(ex, "Bad request on adding item to prescription {PrescriptionId}", id);
//                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error adding item to prescription {PrescriptionId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while adding the item", new[] { ex.Message }, 500));
//            }
//        }

//        /// <summary>
//        /// Remove item from prescription (Doctor only - creator)
//        /// </summary>
//        [HttpDelete("items/{itemId}")]
//        [Authorize(Roles = "Doctor")]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<object>>> RemovePrescriptionItem(Guid itemId)
//        {
//            _logger.LogInformation("Attempting to remove prescription item {ItemId}", itemId);

//            // TODO: Service layer must check that GetCurrentUserId() matches the DoctorId of the prescription this item belongs to.

//            try
//            {
//                var result = await _prescriptionService.RemovePrescriptionItemAsync(itemId);
//                if (!result)
//                {
//                    _logger.LogWarning("Prescription item not found for removal: {ItemId}", itemId);
//                    return NotFound(ApiResponse<object>.Failure($"Prescription item with ID {itemId} not found", statusCode: 404));
//                }

//                _logger.LogInformation("Prescription item {ItemId} removed successfully", itemId);
//                return Ok(ApiResponse<object>.Success(null, "Item removed successfully"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error removing prescription item {ItemId}", itemId);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while removing the item", new[] { ex.Message }, 500));
//            }
//        }

//        #endregion
//    }
//}


