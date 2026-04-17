using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Prescription;
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
    [Route("api/[controller]")]
    [Authorize]
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

        #region Helper Methods

        /// <summary>
        /// Gets the current authenticated user's ID.
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userIdClaim) ? Guid.Empty : Guid.Parse(userIdClaim);
        }

        private bool IsAccessingOwnData(Guid userId)
        {
            var currentUserId = GetCurrentUserId();
            return currentUserId == userId;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        #endregion

        #region CORE CRUD

        /// <summary>
        /// Get prescriptions based on query filters.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionQueryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PrescriptionQueryResponse>>> GetPrescriptions(
            [FromQuery] PrescriptionQueryParams queryParams)
        {
            _logger.LogInformation("Attempting to get prescriptions with query: {@QueryParams}", queryParams);

            // Validation
            if (!queryParams.IsValid(out string validationError))
            {
                _logger.LogWarning("Invalid query parameters for GetPrescriptions: {Error}", validationError);
                return BadRequest(ApiResponse<object>.Failure(validationError, statusCode: 400));
            }

            // TODO: Service layer must implement security logic based on GetCurrentUserId() and roles

            try
            {
                var result = await _prescriptionService.GetPrescriptionsAsync(queryParams);
                return Ok(ApiResponse<PrescriptionQueryResponse>.Success(result, "Prescriptions retrieved successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while getting prescriptions");
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions with filters: {@Filters}", queryParams);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving prescriptions", new[] { ex.Message }, 500));
            }
        }

        /// <summary>
        /// Get prescription by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PrescriptionResponse>>> GetPrescription(Guid id)
        {
            _logger.LogInformation("Attempting to get prescription {PrescriptionId}", id);
            try
            {
                var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id);
                if (prescription == null)
                {
                    _logger.LogWarning("Prescription {PrescriptionId} not found", id);
                    return NotFound(ApiResponse<object>.Failure($"Prescription with ID {id} not found", statusCode: 404));
                }

                // TODO: Service layer must implement security logic based on GetCurrentUserId() and roles

                return Ok(ApiResponse<PrescriptionResponse>.Success(prescription, "Prescription retrieved successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Forbidden: User {UserId} attempted to access prescription {PrescriptionId}", GetCurrentUserId(), id);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("You are not authorized to view this prescription", statusCode: 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription {PrescriptionId}", id);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving the prescription", new[] { ex.Message }, 500));
            }
        }

        /// <summary>
        /// Get prescription by prescription number.
        /// </summary>
        [HttpGet("number/{prescriptionNumber}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PrescriptionResponse>>> GetPrescriptionByNumber(string prescriptionNumber)
        {
            _logger.LogInformation("Attempting to get prescription by number {PrescriptionNumber}", prescriptionNumber);
            try
            {
                var prescription = await _prescriptionService.GetPrescriptionByNumberAsync(prescriptionNumber);
                if (prescription == null)
                {
                    _logger.LogWarning("Prescription with number {PrescriptionNumber} not found", prescriptionNumber);
                    return NotFound(ApiResponse<object>.Failure($"Prescription with number {prescriptionNumber} not found", statusCode: 404));
                }

                // TODO: Security check required

                return Ok(ApiResponse<PrescriptionResponse>.Success(prescription, "Prescription retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription by number {PrescriptionNumber}", prescriptionNumber);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving the prescription", new[] { ex.Message }, 500));
            }
        }

        /// <summary>
        /// Create a new prescription.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PrescriptionResponse>>> CreatePrescription(
            [FromBody] CreatePrescriptionRequest request)
        {
            _logger.LogInformation("Attempting to create prescription for patient {PatientId} by doctor {DoctorId}", request.PatientId, request.DoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for CreatePrescription. Errors: {Errors}", string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
            }

            // Security check: Doctor can only create prescriptions for themself
            var currentUserId = GetCurrentUserId();
            if (currentUserId != request.DoctorId)
            {
                _logger.LogWarning("Forbidden: Doctor {CurrentUserId} attempted to create prescription as doctor {DoctorId}", currentUserId, request.DoctorId);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("Doctors can only create prescriptions as themselves", statusCode: 403));
            }

            try
            {
                var prescription = await _prescriptionService.CreatePrescriptionAsync(request);
                _logger.LogInformation("Prescription {PrescriptionId} created successfully", prescription.Id);

                var response = ApiResponse<PrescriptionResponse>.Success(prescription, "Prescription created successfully", 201);
                return CreatedAtAction(
                    nameof(GetPrescription),
                    new { id = prescription.Id },
                    response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request on prescription creation");
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on prescription creation");
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription");
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while creating the prescription", new[] { ex.Message, ex.InnerException?.Message! }, 500));
            }
        }

        /// <summary>
        /// Update prescription.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PrescriptionResponse>>> UpdatePrescription(
            Guid id,
            [FromBody] UpdatePrescriptionRequest request)
        {
            _logger.LogInformation("Attempting to update prescription {PrescriptionId}", id);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for UpdatePrescription. Errors: {Errors}", string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
            }

            // TODO: Service layer must check that GetCurrentUserId() matches the prescription's DoctorId

            try
            {
                var prescription = await _prescriptionService.UpdatePrescriptionAsync(id, request);
                _logger.LogInformation("Prescription {PrescriptionId} updated successfully", id);
                return Ok(ApiResponse<PrescriptionResponse>.Success(prescription, "Prescription updated successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Prescription not found for update: {PrescriptionId}", id);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Forbidden: User {UserId} attempted to update prescription {PrescriptionId}", GetCurrentUserId(), id);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("You are not authorized to update this prescription", statusCode: 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prescription {PrescriptionId}", id);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while updating the prescription", new[] { ex.Message }, 500));
            }
        }

        /// <summary>
        /// Delete prescription (soft delete).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeletePrescription(Guid id)
        {
            _logger.LogInformation("Attempting to delete prescription {PrescriptionId}", id);

            // TODO: Service layer must check that GetCurrentUserId() matches prescription's DoctorId OR user is Admin

            try
            {
                var result = await _prescriptionService.DeletePrescriptionAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Prescription not found for deletion: {PrescriptionId}", id);
                    return NotFound(ApiResponse<object>.Failure($"Prescription with ID {id} not found", statusCode: 404));
                }

                _logger.LogInformation("Prescription {PrescriptionId} deleted successfully", id);
                return Ok(ApiResponse<object>.Success(null, "Prescription deleted successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Forbidden: User {UserId} attempted to delete prescription {PrescriptionId}", GetCurrentUserId(), id);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("You are not authorized to delete this prescription", statusCode: 403));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while deleting prescription {PrescriptionId}", id);
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prescription {PrescriptionId}", id);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while deleting the prescription", new[] { ex.Message }, 500));
            }
        }

        #endregion

        #region PRESCRIPTION LIFECYCLE

        /// <summary>
        /// Cancel a prescription.
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PrescriptionResponse>>> CancelPrescription(
            Guid id,
            [FromBody] CancelPrescriptionRequest request)
        {
            _logger.LogInformation("Attempting to cancel prescription {PrescriptionId}", id);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for CancelPrescription. Errors: {Errors}", string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
            }

            // TODO: Service layer must check that GetCurrentUserId() matches the prescription's DoctorId

            try
            {
                var result = await _prescriptionService.CancelPrescriptionAsync(id, request);
                _logger.LogInformation("Prescription {PrescriptionId} cancelled successfully", id);
                return Ok(ApiResponse<PrescriptionResponse>.Success(result, "Prescription cancelled successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Prescription not found for cancellation: {PrescriptionId}", id);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Forbidden: User {UserId} attempted to cancel prescription {PrescriptionId}", GetCurrentUserId(), id);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("You are not authorized to cancel this prescription", statusCode: 403));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while cancelling prescription {PrescriptionId}", id);
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling prescription {PrescriptionId}", id);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while cancelling the prescription", new[] { ex.Message }, 500));
            }
        }

        /// <summary>
        /// Renew an existing prescription.
        /// </summary>
        [HttpPost("{id}/renew")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PrescriptionResponse>>> RenewPrescription(
            Guid id,
            [FromBody] RenewPrescriptionRequest request)
        {
            _logger.LogInformation("Attempting to renew prescription {PrescriptionId}", id);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for RenewPrescription. Errors: {Errors}", string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
            }

            // TODO: Service layer must check that GetCurrentUserId() matches the prescription's DoctorId

            try
            {
                var prescription = await _prescriptionService.RenewPrescriptionAsync(id, request);
                _logger.LogInformation("Prescription {PrescriptionId} renewed successfully. New prescription ID: {NewPrescriptionId}", id, prescription.Id);

                var response = ApiResponse<PrescriptionResponse>.Success(prescription, "Prescription renewed successfully", 201);
                return CreatedAtAction(
                    nameof(GetPrescription),
                    new { id = prescription.Id },
                    response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Prescription not found for renewal: {PrescriptionId}", id);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Forbidden: User {UserId} attempted to renew prescription {PrescriptionId}", GetCurrentUserId(), id);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("You are not authorized to renew this prescription", statusCode: 403));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while renewing prescription {PrescriptionId}", id);
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing prescription {PrescriptionId}", id);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while renewing the prescription", new[] { ex.Message, ex.InnerException?.Message! }, 500));
            }
        }

        #endregion

        #region PHARMACY OPERATIONS

        /// <summary>
        /// Verify prescription authenticity (for pharmacies).
        /// </summary>
        //[HttpGet("{id}/verify")]
        //[Authorize(Roles = "Pharmacy,Pharmacist")]
        //[ProducesResponseType(typeof(ApiResponse<PrescriptionVerificationResponse>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<ApiResponse<PrescriptionVerificationResponse>>> VerifyPrescription(
        //    Guid id,
        //    [FromQuery] string? verificationCode = null)
        //{
        //    _logger.LogInformation("Attempting to verify prescription {PrescriptionId} by user {UserId}", id, GetCurrentUserId());
        //    try
        //    {
        //        var result = await _prescriptionService.VerifyPrescriptionAsync(id, verificationCode);
        //        _logger.LogInformation("Verification successful for prescription {PrescriptionId}", id);
        //        return Ok(ApiResponse<PrescriptionVerificationResponse>.Success(result, "Verification successful"));
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LogWarning(ex, "Prescription not found for verification: {PrescriptionId}", id);
        //        return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error verifying prescription {PrescriptionId}", id);
        //        return StatusCode(500, ApiResponse<object>.Failure("An error occurred while verifying the prescription", new[] { ex.Message }, 500));
        //    }
        //}

        /// <summary>
        /// Mark prescription as dispensed by pharmacy.
        /// </summary>
        //[HttpPost("{id}/dispense")]
        //[Authorize(Roles = "Pharmacy,Pharmacist")]
        //[ProducesResponseType(typeof(ApiResponse<DispenseResult>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<ApiResponse<DispenseResult>>> DispensePrescription(
        //    Guid id,
        //    [FromBody] DispensePrescriptionRequest request)
        //{
        //    _logger.LogInformation("Attempting to dispense prescription {PrescriptionId} by pharmacy {PharmacyId}", id, request.PharmacyId);

        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //        _logger.LogWarning("Invalid model state for DispensePrescription. Errors: {Errors}", string.Join(", ", errors));
        //        return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
        //    }

        //    // TODO: Security check: User must be part of the request.PharmacyId or Admin

        //    try
        //    {
        //        var result = await _prescriptionService.DispensePrescriptionAsync(id, request);
        //        _logger.LogInformation("Prescription {PrescriptionId} dispensed successfully", id);
        //        return Ok(ApiResponse<DispenseResult>.Success(result, "Prescription dispensed successfully"));
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LogWarning(ex, "Prescription not found for dispensing: {PrescriptionId}", id);
        //        return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LogWarning(ex, "Invalid operation while dispensing prescription {PrescriptionId}", id);
        //        return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error dispensing prescription {PrescriptionId}", id);
        //        return StatusCode(500, ApiResponse<object>.Failure("An error occurred while dispensing the prescription", new[] { ex.Message }, 500));
        //    }
        //}

        /// <summary>
        /// Mark prescription as digitally shared with pharmacy.
        /// </summary>
        //[HttpPost("{id}/share")]
        //[Authorize(Roles = "Doctor,Patient")]
        //[ProducesResponseType(typeof(ApiResponse<SharePrescriptionResult>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<ApiResponse<SharePrescriptionResult>>> SharePrescription(
        //    Guid id,
        //    [FromBody] SharePrescriptionRequest request)
        //{
        //    _logger.LogInformation("Attempting to share prescription {PrescriptionId} with pharmacy {PharmacyId}", id, request.PharmacyId);

        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //        _logger.LogWarning("Invalid model state for SharePrescription. Errors: {Errors}", string.Join(", ", errors));
        //        return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
        //    }

        //    // TODO: Service layer must check that GetCurrentUserId() matches prescription's DoctorId or PatientId

        //    try
        //    {
        //        var result = await _prescriptionService.SharePrescriptionAsync(id, request);
        //        _logger.LogInformation("Prescription {PrescriptionId} shared successfully with {PharmacyId}", id, request.PharmacyId);
        //        return Ok(ApiResponse<SharePrescriptionResult>.Success(result, "Prescription shared successfully"));
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LogWarning(ex, "Prescription or Pharmacy not found for sharing: {PrescriptionId}", id);
        //        return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        _logger.LogWarning("Forbidden: User {UserId} attempted to share prescription {PrescriptionId}", GetCurrentUserId(), id);
        //        return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("You are not authorized to share this prescription", statusCode: 403));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error sharing prescription {PrescriptionId}", id);
        //        return StatusCode(500, ApiResponse<object>.Failure("An error occurred while sharing the prescription", new[] { ex.Message }, 500));
        //    }
        //}

        ///// <summary>
        ///// Get prescription dispensing history.
        ///// </summary>
        //[HttpGet("{id}/dispensing-history")]
        //[Authorize(Roles = "Doctor,Pharmacy,Admin")]
        //[ProducesResponseType(typeof(ApiResponse<IEnumerable<DispensingRecord>>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<ApiResponse<IEnumerable<DispensingRecord>>>> GetDispensingHistory(Guid id)
        //{
        //    _logger.LogInformation("Attempting to get dispensing history for prescription {PrescriptionId}", id);

        //    // TODO: Service layer must check user authorization

        //    try
        //    {
        //        var history = await _prescriptionService.GetDispensingHistoryAsync(id);
        //        _logger.LogInformation("Retrieved {Count} history records for prescription {PrescriptionId}", history.Count(), id);
        //        return Ok(ApiResponse<IEnumerable<DispensingRecord>>.Success(history, "Dispensing history retrieved successfully"));
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LogWarning(ex, "Prescription not found for history: {PrescriptionId}", id);
        //        return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting dispensing history for prescription {PrescriptionId}", id);
        //        return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving dispensing history", new[] { ex.Message }, 500));
        //    }
        //}

        /// <summary>
        /// Accept prescription delivery to pharmacy.
        /// </summary>
        //[HttpPost("{id}/accept-delivery")]
        //[Authorize(Roles = "Pharmacy,Pharmacist")]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<ApiResponse<object>>> AcceptPrescriptionDelivery(
        //    Guid id,
        //    [FromBody] AcceptDeliveryRequest request)
        //{
        //    _logger.LogInformation("Attempting to accept delivery for prescription {PrescriptionId} by pharmacy {PharmacyId}", id, request.PharmacyId);

        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //        _logger.LogWarning("Invalid model state for AcceptPrescriptionDelivery. Errors: {Errors}", string.Join(", ", errors));
        //        return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
        //    }

        //    // TODO: Security check: User must be part of the request.PharmacyId or Admin

        //    try
        //    {
        //        await _prescriptionService.AcceptPrescriptionDeliveryAsync(id, request);

        //        _logger.LogInformation("Delivery accepted for prescription {PrescriptionId} by pharmacy {PharmacyId}", id, request.PharmacyId);

        //        // Get prescription details to return
        //        var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id);

        //        var responseData = new
        //        {
        //            Message = "Prescription accepted for delivery",
        //            Success = true,
        //            PrescriptionId = id,
        //            PharmacyId = request.PharmacyId,
        //            EstimatedDeliveryMinutes = request.EstimatedDeliveryMinutes,
        //            DeliveryAddress = request.DeliveryAddress,
        //            DeliveryFee = request.DeliveryFee,
        //            DeliveryNotes = request.DeliveryNotes,
        //            Prescription = prescription
        //        };

        //        return Ok(ApiResponse<object>.Success(responseData, "Prescription accepted for delivery"));
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LogWarning(ex, "Prescription or Pharmacy not found for delivery acceptance: {PrescriptionId}", id);
        //        return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LogWarning(ex, "Invalid operation while accepting delivery for prescription {PrescriptionId}", id);
        //        return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error accepting delivery for prescription {PrescriptionId}", id);
        //        return StatusCode(500, ApiResponse<object>.Failure("An error occurred while accepting delivery", new[] { ex.Message }, 500));
        //    }
        //}

        #endregion

        #region PATIENT SPECIALIZED ENDPOINTS

        /// <summary>
        /// Get patient's current active medications.
        /// </summary>
        [HttpGet("patient/{patientId}/current-medications")]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CurrentMedicationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CurrentMedicationResponse>>>> GetCurrentMedications(
            Guid patientId)
        {
            _logger.LogInformation("Attempting to get current medications for patient {PatientId}", patientId);

            // Security Check
            var currentUserId = GetCurrentUserId();
            if (User.IsInRole("Patient") && !IsAdmin() && currentUserId != patientId)
            {
                _logger.LogWarning("Forbidden: Patient {CurrentUserId} attempted to access medications of patient {PatientId}", currentUserId, patientId);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("Patients can only view their own medications", statusCode: 403));
            }
            // TODO: Add check for Doctor's relation to patient

            try
            {
                var medications = await _prescriptionService.GetCurrentMedicationsAsync(patientId);
                _logger.LogInformation("Retrieved {Count} current medications for patient {PatientId}", medications.Count(), patientId);
                return Ok(ApiResponse<IEnumerable<CurrentMedicationResponse>>.Success(medications, "Current medications retrieved successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found for GetCurrentMedications: {PatientId}", patientId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Forbidden: User {UserId} attempted to get medications for patient {PatientId}", GetCurrentUserId(), patientId);
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("You are not authorized to view this information", statusCode: 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current medications for patient {PatientId}", patientId);
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving current medications", new[] { ex.Message }, 500));
            }
        }

        #endregion

        #region PATIENT-DOCTOR PRESCRIPTIONS

        /// <summary>
        /// Get a specific prescription between patient and doctor with full medication details.
        /// </summary>
        [HttpGet("patient/{patientId}/doctor/{doctorId}/prescription/{prescriptionId}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionDetailedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PrescriptionDetailedResponse>>> GetPrescriptionBetweenPatientAndDoctor(
            Guid patientId,
            Guid doctorId,
            Guid prescriptionId)
        {
            _logger.LogInformation(
                "Attempting to get prescription {PrescriptionId} between patient {PatientId} and doctor {DoctorId}",
                prescriptionId, patientId, doctorId);

            try
            {
                var prescription = await _prescriptionService.GetPrescriptionBetweenPatientAndDoctorAsync(
                    prescriptionId, patientId, doctorId);

                if (prescription == null)
                {
                    _logger.LogWarning(
                        "Prescription {PrescriptionId} not found between patient {PatientId} and doctor {DoctorId}",
                        prescriptionId, patientId, doctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Prescription not found or does not belong to the specified patient and doctor",
                        statusCode: 404));
                }

                return Ok(ApiResponse<PrescriptionDetailedResponse>.Success(
                    prescription,
                    "Prescription retrieved successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning(
                    "Forbidden: User {UserId} attempted to access prescription {PrescriptionId}",
                    GetCurrentUserId(), prescriptionId);
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Failure("You are not authorized to view this prescription", statusCode: 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting prescription {PrescriptionId} between patient {PatientId} and doctor {DoctorId}",
                    prescriptionId, patientId, doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving the prescription",
                    new[] { ex.Message }, 500));
            }
        }

        /// <summary>
        /// Get all prescriptions between patient and doctor (summary list).
        /// </summary>
        [HttpGet("patient/{patientId}/doctor/{doctorId}/list")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PrescriptionListItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PrescriptionListItemResponse>>>> GetPrescriptionListBetweenPatientAndDoctor(
            Guid patientId,
            Guid doctorId)
        {
            _logger.LogInformation(
                "Attempting to get prescription list between patient {PatientId} and doctor {DoctorId}",
                patientId, doctorId);

            try
            {
                var prescriptions = await _prescriptionService.GetPrescriptionListBetweenPatientAndDoctorAsync(
                    patientId, doctorId);

                _logger.LogInformation(
                    "Retrieved {Count} prescriptions between patient {PatientId} and doctor {DoctorId}",
                    prescriptions.Count(), patientId, doctorId);

                return Ok(ApiResponse<IEnumerable<PrescriptionListItemResponse>>.Success(
                    prescriptions,
                    "Prescription list retrieved successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning(
                    "Forbidden: User {UserId} attempted to access prescriptions between patient {PatientId} and doctor {DoctorId}",
                    GetCurrentUserId(), patientId, doctorId);
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Failure("You are not authorized to view these prescriptions", statusCode: 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting prescription list between patient {PatientId} and doctor {DoctorId}",
                    patientId, doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving the prescription list",
                    new[] { ex.Message }, 500));
            }
        }

        #endregion

        #region MEDICATIONS

        /// <summary>
        /// Get all medication names available in the system with optional search.
        /// </summary>
        /// <param name="search">Optional search term to filter medications by brand name or generic name</param>
        [HttpGet("medications")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<MedicationNameResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<MedicationNameResponse>>>> GetAllMedicationNames(
            [FromQuery] string? search = null)
        {
            _logger.LogInformation("Attempting to get medication names with search: {SearchTerm}", search ?? "none");

            try
            {
                var medications = await _prescriptionService.GetAllMedicationNamesAsync(search);
                
                _logger.LogInformation("Retrieved {Count} medication names", medications.Count());
                
                return Ok(ApiResponse<IEnumerable<MedicationNameResponse>>.Success(
                    medications,
                    "Medication names retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medication names with search: {SearchTerm}", search);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while retrieving medication names",
                    new[] { ex.Message }, 500));
            }
        }

        #endregion

        #region ANALYTICS & STATISTICS

        /// <summary>
        /// Get prescription status history (audit trail).
        /// </summary>
        //[HttpGet("{id}/status-history")]
        //[Authorize(Roles = "Doctor,Admin")]
        //[ProducesResponseType(typeof(ApiResponse<IEnumerable<PrescriptionStatusHistory>>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<ApiResponse<IEnumerable<PrescriptionStatusHistory>>>> GetStatusHistory(Guid id)
        //{
        //    _logger.LogInformation("Attempting to get status history for prescription {PrescriptionId}", id);

        //    // TODO: Service layer must check that GetCurrentUserId() matches prescription's DoctorId OR user is Admin

        //    try
        //    {
        //        var history = await _prescriptionService.GetStatusHistoryAsync(id);
        //        _logger.LogInformation("Retrieved {Count} status history records for prescription {PrescriptionId}", history.Count(), id);
        //        return Ok(ApiResponse<IEnumerable<PrescriptionStatusHistory>>.Success(history, "Status history retrieved successfully"));
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LogWarning(ex, "Prescription not found for GetStatusHistory: {PrescriptionId}", id);
        //        return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting status history for prescription {PrescriptionId}", id);
        //        return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving status history", new[] { ex.Message }, 500));
        //    }
        //}

        #endregion
    }
}
