//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using Shuryan.Application.DTOs.Common.Base;
//using Shuryan.Application.DTOs.Requests.Laboratory;
//using Shuryan.Application.DTOs.Responses.Laboratory;
//using Shuryan.Application.Interfaces;
//using Shuryan.Core.Enums.Laboratory;
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
//    public class LabOrdersController : ControllerBase
//    {
//        private readonly ILabOrderService _labOrderService;
//        private readonly ILogger<LabOrdersController> _logger;

//        public LabOrdersController(
//            ILabOrderService labOrderService,
//            ILogger<LabOrdersController> logger)
//        {
//            _labOrderService = labOrderService;
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
//        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabOrderResponse>>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<IEnumerable<LabOrderResponse>>>> GetAllLabOrders(
//            [FromQuery] Guid? patientId = null,
//            [FromQuery] Guid? laboratoryId = null,
//            [FromQuery] LabOrderStatus? status = null,
//            [FromQuery] DateTime? startDate = null,
//            [FromQuery] DateTime? endDate = null)
//        {
//            _logger.LogInformation("Admin user {AdminId} getting all lab orders with filters", GetCurrentUserId());
            
//            try
//            {
//                var orders = await _labOrderService.GetAllLabOrdersAsync(
//                    patientId,
//                    laboratoryId,
//                    status,
//                    startDate,
//                    endDate);
                
//                _logger.LogInformation("Successfully retrieved {Count} lab orders", orders.Count());
//                return Ok(ApiResponse<IEnumerable<LabOrderResponse>>.Success(
//                    orders,
//                    "Lab orders retrieved successfully"
//                ));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error retrieving all lab orders");
//                return StatusCode(500, ApiResponse<object>.Failure(
//                    "An unexpected error occurred while retrieving lab orders",
//                    new[] { ex.Message },
//                    500
//                ));
//            }
//        }

//        /// <summary>
//        /// Get lab order by ID. Accessible by owner (Patient, Lab) or Admin/Doctor.
//        /// </summary>
//        [HttpGet("{id}")]
//        [Authorize(Roles = "Patient,Doctor,Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<LabOrderResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabOrderResponse>>> GetLabOrder(Guid id)
//        {
//            _logger.LogInformation("Attempting to get lab order {OrderId}", id);
//            try
//            {
//                var order = await _labOrderService.GetLabOrderByIdAsync(id);
//                if (order == null)
//                {
//                    _logger.LogWarning("Lab order not found: {OrderId}", id);
//                    return NotFound(ApiResponse<object>.Failure($"Lab order with ID {id} not found", statusCode: 404));
//                }

//                // TODO: Add service-layer check if current user (GetCurrentUserId())
//                // is the Patient, the Laboratory, a Doctor, or Admin.
//                // For now, we assume the role check is sufficient for controller access.

//                return Ok(ApiResponse<LabOrderResponse>.Success(order, "Lab order retrieved successfully"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting lab order {OrderId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving the order", new[] { ex.Message }, 500));
//            }
//        }

//        [HttpGet("patient/{patientId}")]
//        [Authorize(Roles = "Patient,Doctor,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabOrderResponse>>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<IEnumerable<LabOrderResponse>>>> GetPatientLabOrders(Guid patientId)
//        {
//            _logger.LogInformation("Get lab orders request for patient: {PatientId}", patientId);

//            // Ownership check: Patients can only view their own lab orders unless they're Admin or Doctor
//            if (User.IsInRole("Patient") && !IsAdmin() && !IsAccessingOwnData(patientId))
//            {
//                _logger.LogWarning("Forbidden: Patient {CurrentUserId} attempted to access orders of patient {PatientId}", 
//                    GetCurrentUserId(), patientId);
//                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure(
//                    "Patients can only view their own lab orders",
//                    statusCode: 403
//                ));
//            }

//            try
//            {
//                var orders = await _labOrderService.GetPatientLabOrdersAsync(patientId);
//                _logger.LogInformation("Successfully retrieved {Count} lab orders for patient: {PatientId}", orders.Count(), patientId);
//                return Ok(ApiResponse<IEnumerable<LabOrderResponse>>.Success(
//                    orders,
//                    "Patient lab orders retrieved successfully"
//                ));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error retrieving lab orders for patient {PatientId}", patientId);
//                return StatusCode(500, ApiResponse<object>.Failure(
//                    "An unexpected error occurred while retrieving orders",
//                    new[] { ex.Message },
//                    500
//                ));
//            }
//        }

//        [HttpGet("laboratory/{laboratoryId}")]
//        [Authorize(Roles = "Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabOrderResponse>>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<IEnumerable<LabOrderResponse>>>> GetLaboratoryLabOrders(Guid laboratoryId)
//        {
//            _logger.LogInformation("Get lab orders request for laboratory: {LaboratoryId}", laboratoryId);

//            // Ownership check: Laboratories can only view their own lab orders unless they're Admin
//            if (!IsAdmin() && !IsAccessingOwnData(laboratoryId))
//            {
//                _logger.LogWarning("Forbidden: Laboratory {CurrentUserId} attempted to access orders of laboratory {LaboratoryId}", 
//                    GetCurrentUserId(), laboratoryId);
//                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure(
//                    "Laboratories can only view their own lab orders",
//                    statusCode: 403
//                ));
//            }

//            try
//            {
//                var orders = await _labOrderService.GetLaboratoryLabOrdersAsync(laboratoryId);
//                _logger.LogInformation("Successfully retrieved {Count} lab orders for laboratory: {LaboratoryId}", orders.Count(), laboratoryId);
//                return Ok(ApiResponse<IEnumerable<LabOrderResponse>>.Success(
//                    orders,
//                    "Laboratory lab orders retrieved successfully"
//                ));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error retrieving lab orders for laboratory {LaboratoryId}", laboratoryId);
//                return StatusCode(500, ApiResponse<object>.Failure(
//                    "An unexpected error occurred while retrieving orders",
//                    new[] { ex.Message },
//                    500
//                ));
//            }
//        }

//        [HttpPost]
//        [Authorize(Roles = "Patient,Doctor")]
//        [ProducesResponseType(typeof(ApiResponse<LabOrderResponse>), StatusCodes.Status201Created)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabOrderResponse>>> CreateLabOrder(
//            [FromBody] CreateLabOrderRequest request)
//        {
//            _logger.LogInformation("Create lab order request for patient: {PatientId}", request.PatientId);

//            if (!ModelState.IsValid)
//            {
//                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
//                _logger.LogWarning("Invalid model state for CreateLabOrder. Errors: {Errors}", string.Join(", ", errors));
//                return BadRequest(ApiResponse<object>.Failure(
//                    "Invalid request data",
//                    errors,
//                    400
//                ));
//            }

//            try
//            {
//                var order = await _labOrderService.CreateLabOrderAsync(request);
//                _logger.LogInformation("Lab order {OrderId} created successfully", order.Id);

//                var response = ApiResponse<LabOrderResponse>.Success(
//                    order,
//                    "Lab order created successfully",
//                    201
//                );
//                return CreatedAtAction(
//                    nameof(GetLabOrder),
//                    new { id = order.Id },
//                    response);
//            }
//            catch (ArgumentException ex)
//            {
//                _logger.LogWarning(ex, "Bad request on lab order creation");
//                return BadRequest(ApiResponse<object>.Failure(
//                    ex.Message,
//                    statusCode: 400
//                ));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error creating lab order");
//                return StatusCode(500, ApiResponse<object>.Failure(
//                    "An unexpected error occurred while creating the lab order",
//                    new[] { ex.Message },
//                    500
//                ));
//            }
//        }

//        /// <summary>
//        /// Cancel lab order. Accessible by Patient, Laboratory, or Admin.
//        /// </summary>
//        [HttpPost("{id}/cancel")]
//        [Authorize(Roles = "Patient,Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<LabOrderResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabOrderResponse>>> CancelLabOrder(
//            Guid id,
//            [FromBody] CancelLabOrderRequest request)
//        {
//            _logger.LogInformation("Attempting to cancel lab order {OrderId}", id);

//            if (!ModelState.IsValid)
//            {
//                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
//                _logger.LogWarning("Invalid model state for CancelLabOrder. Errors: {Errors}", string.Join(", ", errors));
//                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
//            }

//            try
//            {
//                // Service layer should check if the current user has permission
//                // and if the order is in a state that can be cancelled.
//                var order = await _labOrderService.CancelLabOrderAsync(id, request.CancellationReason);
//                _logger.LogInformation("Lab order {OrderId} cancelled successfully", id);
//                return Ok(ApiResponse<LabOrderResponse>.Success(order, "Lab order cancelled successfully"));
//            }
//            catch (ArgumentException ex) // Order not found
//            {
//                _logger.LogWarning(ex, "Lab order not found for cancellation: {OrderId}", id);
//                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
//            }
//            catch (InvalidOperationException ex) // Order cannot be cancelled
//            {
//                _logger.LogWarning(ex, "Invalid operation on lab order cancellation: {OrderId}", id);
//                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error cancelling lab order {OrderId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while cancelling the order", new[] { ex.Message }, 500));
//            }
//        }

//        /// <summary>
//        /// Delete lab order (soft delete). Admin only.
//        /// </summary>
//        [HttpDelete("{id}")]
//        [Authorize(Roles = "Admin")]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<object>>> DeleteLabOrder(Guid id)
//        {
//            _logger.LogInformation("Attempting to delete lab order {OrderId}", id);
//            try
//            {
//                var result = await _labOrderService.DeleteLabOrderAsync(id);
//                if (!result)
//                {
//                    _logger.LogWarning("Lab order not found for deletion: {OrderId}", id);
//                    return NotFound(ApiResponse<object>.Failure($"Lab order with ID {id} not found", statusCode: 404));
//                }

//                _logger.LogInformation("Lab order {OrderId} deleted successfully", id);
//                return Ok(ApiResponse<object>.Success(null, "Lab order deleted successfully"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error deleting lab order {OrderId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while deleting the order", new[] { ex.Message }, 500));
//            }
//        }

//        #endregion

//        #region Order Lifecycle

//        /// <summary>
//        /// Confirm lab order by laboratory.
//        /// </summary>
//        [HttpPost("{id}/confirm")]
//        [Authorize(Roles = "Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<LabOrderResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabOrderResponse>>> ConfirmLabOrder(Guid id)
//        {
//            _logger.LogInformation("Attempting to confirm lab order {OrderId}", id);
//            try
//            {
//                // Service layer should check if current user is the correct laboratory
//                var order = await _labOrderService.ConfirmLabOrderAsync(id);
//                _logger.LogInformation("Lab order {OrderId} confirmed", id);
//                return Ok(ApiResponse<LabOrderResponse>.Success(order, "Lab order confirmed"));
//            }
//            catch (ArgumentException ex) // Not found
//            {
//                _logger.LogWarning(ex, "Lab order not found for confirmation: {OrderId}", id);
//                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
//            }
//            catch (InvalidOperationException ex) // Invalid state
//            {
//                _logger.LogWarning(ex, "Invalid operation on lab order confirmation: {OrderId}", id);
//                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error confirming lab order {OrderId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while confirming the order", new[] { ex.Message }, 500));
//            }
//        }

//        /// <summary>
//        /// Mark order as sample collected.
//        /// </summary>
//        [HttpPost("{id}/sample-collected")]
//        [Authorize(Roles = "Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<LabOrderResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabOrderResponse>>> MarkSampleCollected(Guid id)
//        {
//            _logger.LogInformation("Attempting to mark sample collected for lab order {OrderId}", id);
//            try
//            {
//                var order = await _labOrderService.MarkSampleCollectedAsync(id);
//                _logger.LogInformation("Lab order {OrderId} marked as sample collected", id);
//                return Ok(ApiResponse<LabOrderResponse>.Success(order, "Sample collected status updated"));
//            }
//            catch (ArgumentException ex) // Not found
//            {
//                _logger.LogWarning(ex, "Lab order not found for sample collected: {OrderId}", id);
//                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
//            }
//            catch (InvalidOperationException ex) // Invalid state
//            {
//                _logger.LogWarning(ex, "Invalid operation on mark sample collected: {OrderId}", id);
//                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error marking sample collected for lab order {OrderId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while updating status", new[] { ex.Message }, 500));
//            }
//        }

//        /// <summary>
//        /// Mark order as in progress (tests being performed).
//        /// </summary>
//        [HttpPost("{id}/in-progress")]
//        [Authorize(Roles = "Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<LabOrderResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabOrderResponse>>> MarkInProgress(Guid id)
//        {
//            _logger.LogInformation("Attempting to mark in progress for lab order {OrderId}", id);
//            try
//            {
//                var order = await _labOrderService.MarkInProgressAsync(id);
//                _logger.LogInformation("Lab order {OrderId} marked as in progress", id);
//                return Ok(ApiResponse<LabOrderResponse>.Success(order, "In progress status updated"));
//            }
//            catch (ArgumentException ex) // Not found
//            {
//                _logger.LogWarning(ex, "Lab order not found for in progress: {OrderId}", id);
//                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
//            }
//            catch (InvalidOperationException ex) // Invalid state
//            {
//                _logger.LogWarning(ex, "Invalid operation on mark in progress: {OrderId}", id);
//                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error marking lab order {OrderId} as in progress", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while updating status", new[] { ex.Message }, 500));
//            }
//        }

//        /// <summary>
//        /// Complete lab order (all results ready).
//        /// </summary>
//        [HttpPost("{id}/complete")]
//        [Authorize(Roles = "Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<LabOrderResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabOrderResponse>>> CompleteLabOrder(Guid id)
//        {
//            _logger.LogInformation("Attempting to complete lab order {OrderId}", id);
//            try
//            {
//                var order = await _labOrderService.CompleteLabOrderAsync(id);
//                _logger.LogInformation("Lab order {OrderId} completed", id);
//                return Ok(ApiResponse<LabOrderResponse>.Success(order, "Lab order completed"));
//            }
//            catch (ArgumentException ex) // Not found
//            {
//                _logger.LogWarning(ex, "Lab order not found for completion: {OrderId}", id);
//                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
//            }
//            catch (InvalidOperationException ex) // Invalid state
//            {
//                _logger.LogWarning(ex, "Invalid operation on lab order completion: {OrderId}", id);
//                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error completing lab order {OrderId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while completing the order", new[] { ex.Message }, 500));
//            }
//        }

//        /// <summary>
//        /// Mark lab order as paid.
//        /// </summary>
//        [HttpPost("{id}/mark-paid")]
//        [Authorize(Roles = "Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<LabOrderResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabOrderResponse>>> MarkLabOrderAsPaid(
//            Guid id,
//            [FromBody] MarkAsPaidRequest request)
//        {
//            _logger.LogInformation("Attempting to mark lab order {OrderId} as paid", id);

//            if (!ModelState.IsValid)
//            {
//                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
//                _logger.LogWarning("Invalid model state for MarkLabOrderAsPaid. Errors: {Errors}", string.Join(", ", errors));
//                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
//            }

//            try
//            {
//                var order = await _labOrderService.MarkLabOrderAsPaidAsync(id, request.PaymentMethod, request.TransactionId);
//                _logger.LogInformation("Lab order {OrderId} marked as paid", id);
//                return Ok(ApiResponse<LabOrderResponse>.Success(order, "Lab order marked as paid"));
//            }
//            catch (ArgumentException ex) // Not found
//            {
//                _logger.LogWarning(ex, "Lab order not found for marking as paid: {OrderId}", id);
//                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
//            }
//            catch (InvalidOperationException ex) // Invalid state
//            {
//                _logger.LogWarning(ex, "Invalid operation on marking lab order as paid: {OrderId}", id);
//                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error marking lab order {OrderId} as paid", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while marking as paid", new[] { ex.Message }, 500));
//            }
//        }

//        #endregion

//        #region Results Management

//        /// <summary>
//        /// Get lab order results. Accessible by Patient, Doctor, Laboratory, Admin.
//        /// </summary>
//        [HttpGet("{id}/results")]
//        [Authorize(Roles = "Patient,Doctor,Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabResultResponse>>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<IEnumerable<LabResultResponse>>>> GetLabOrderResults(Guid id)
//        {
//            _logger.LogInformation("Attempting to get results for lab order {OrderId}", id);
//            try
//            {
//                // TODO: Service-layer check to ensure user has permission for this order
//                var results = await _labOrderService.GetLabOrderResultsAsync(id);
//                return Ok(ApiResponse<IEnumerable<LabResultResponse>>.Success(results, "Lab order results retrieved successfully"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting results for lab order {OrderId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving results", new[] { ex.Message }, 500));
//            }
//        }

//        /// <summary>
//        /// Add result to lab order. Accessible by Laboratory or Admin.
//        /// </summary>
//        [HttpPost("{id}/results")]
//        [Authorize(Roles = "Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<LabResultResponse>), StatusCodes.Status201Created)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabResultResponse>>> AddLabOrderResult(
//            Guid id,
//            [FromBody] CreateLabResultRequest request)
//        {
//            _logger.LogInformation("Attempting to add result to lab order {OrderId}", id);

//            if (!ModelState.IsValid)
//            {
//                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
//                _logger.LogWarning("Invalid model state for AddLabOrderResult. Errors: {Errors}", string.Join(", ", errors));
//                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
//            }

//            try
//            {
//                // TODO: Service-layer check if user is the correct laboratory for this order
//                var result = await _labOrderService.AddLabOrderResultAsync(id, request);
//                _logger.LogInformation("Result {ResultId} added to lab order {OrderId}", result.Id, id);

//                var response = ApiResponse<LabResultResponse>.Success(result, "Result added successfully", 201);
//                return CreatedAtAction(
//                    nameof(GetLabOrderResults), // TODO: Needs a "GetResultById" endpoint
//                    new { id },
//                    response);
//            }
//            catch (ArgumentException ex) // e.g., Order not found, or test not part of order
//            {
//                _logger.LogWarning(ex, "Bad request on adding lab result to order {OrderId}", id);
//                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error adding result to lab order {OrderId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while adding the result", new[] { ex.Message }, 500));
//            }
//        }

//        /// <summary>
//        /// Update lab result. Accessible by Laboratory or Admin.
//        /// </summary>
//        [HttpPut("results/{resultId}")]
//        [Authorize(Roles = "Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<LabResultResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabResultResponse>>> UpdateLabResult(
//            Guid resultId,
//            [FromBody] UpdateLabResultRequest request)
//        {
//            _logger.LogInformation("Attempting to update lab result {ResultId}", resultId);

//            if (!ModelState.IsValid)
//            {
//                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
//                _logger.LogWarning("Invalid model state for UpdateLabResult. Errors: {Errors}", string.Join(", ", errors));
//                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
//            }

//            try
//            {
//                // TODO: Service-layer check if user is the correct laboratory for this result
//                var result = await _labOrderService.UpdateLabResultAsync(resultId, request);
//                _logger.LogInformation("Lab result {ResultId} updated successfully", resultId);
//                return Ok(ApiResponse<LabResultResponse>.Success(result, "Result updated successfully"));
//            }
//            catch (ArgumentException ex) // Result not found
//            {
//                _logger.LogWarning(ex, "Lab result not found for update: {ResultId}", resultId);
//                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error updating lab result {ResultId}", resultId);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while updating the result", new[] { ex.Message }, 500));
//            }
//        }

//        #endregion

//        #region Statistics

//        /// <summary>
//        /// Get lab order statistics. Accessible by Laboratory or Admin.
//        /// </summary>
//        [HttpGet("statistics")]
//        [Authorize(Roles = "Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<LabOrderStatistics>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LabOrderStatistics>>> GetLabOrderStatistics(
//            [FromQuery] Guid? laboratoryId = null,
//            [FromQuery] DateTime? startDate = null,
//            [FromQuery] DateTime? endDate = null)
//        {
//            _logger.LogInformation("Attempting to get lab order statistics. LabID: {LaboratoryId}", laboratoryId);

//            // Security check: If a lab user is asking, they can only see their own stats.
//            var currentUserId = GetCurrentUserId();
//            if (User.IsInRole("Laboratory") && !IsAdmin())
//            {
//                if (laboratoryId == null)
//                {
//                    // If lab user didn't specify ID, force it to be their own ID.
//                    laboratoryId = currentUserId;
//                }
//                else if (laboratoryId != currentUserId)
//                {
//                    _logger.LogWarning("Forbidden: Laboratory {CurrentUserId} attempted to access statistics for {LaboratoryId}", currentUserId, laboratoryId);
//                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("Laboratories can only view their own statistics", statusCode: 403));
//                }
//            }

//            try
//            {
//                var statistics = await _labOrderService.GetLabOrderStatisticsAsync(laboratoryId, startDate, endDate);
//                return Ok(ApiResponse<LabOrderStatistics>.Success(statistics, "Statistics retrieved successfully"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting lab order statistics");
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving statistics", new[] { ex.Message }, 500));
//            }
//        }

//        #endregion
//    }

//    // Helper request models
//    public class CancelLabOrderRequest
//    {
//        public string CancellationReason { get; set; } = string.Empty;
//    }

//    public class MarkAsPaidRequest
//    {
//        public string PaymentMethod { get; set; } = string.Empty;
//        public string? TransactionId { get; set; }
//    }
//}

