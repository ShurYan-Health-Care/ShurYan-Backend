using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class LabOrdersController : ControllerBase
    {
        private readonly ILabOrderService _labOrderService;
        private readonly ILogger<LabOrdersController> _logger;

        public LabOrdersController(
            ILabOrderService labOrderService,
            ILogger<LabOrdersController> logger)
        {
            _labOrderService = labOrderService;
            _logger = logger;
        }

        // ==================== CRUD Operations ====================

        /// <summary>
        /// Get all lab orders with optional filters
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LabOrderResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LabOrderResponse>>> GetAllLabOrders(
            [FromQuery] Guid? patientId = null,
            [FromQuery] Guid? laboratoryId = null,
            [FromQuery] LabOrderStatus? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var orders = await _labOrderService.GetAllLabOrdersAsync(
                    patientId,
                    laboratoryId,
                    status,
                    startDate,
                    endDate);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all lab orders");
                return StatusCode(500, new { Message = "An error occurred while retrieving lab orders" });
            }
        }

        /// <summary>
        /// Get lab order by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LabOrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LabOrderResponse>> GetLabOrder(Guid id)
        {
            try
            {
                var order = await _labOrderService.GetLabOrderByIdAsync(id);
                if (order == null)
                    return NotFound(new { Message = $"Lab order with ID {id} not found" });

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab order {OrderId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get patient's lab orders
        /// </summary>
        [HttpGet("patient/{patientId}")]
        //[Authorize(Roles = "Patient,Doctor,Admin")]
        [ProducesResponseType(typeof(IEnumerable<LabOrderResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LabOrderResponse>>> GetPatientLabOrders(Guid patientId)
        {
            try
            {
                var orders = await _labOrderService.GetPatientLabOrdersAsync(patientId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab orders for patient {PatientId}", patientId);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get laboratory's lab orders
        /// </summary>
        [HttpGet("laboratory/{laboratoryId}")]
        //[Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(typeof(IEnumerable<LabOrderResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LabOrderResponse>>> GetLaboratoryLabOrders(Guid laboratoryId)
        {
            try
            {
                var orders = await _labOrderService.GetLaboratoryLabOrdersAsync(laboratoryId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab orders for laboratory {LaboratoryId}", laboratoryId);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Create a new lab order
        /// </summary>
        [HttpPost]
        //[Authorize(Roles = "Patient,Doctor")]
        [ProducesResponseType(typeof(LabOrderResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LabOrderResponse>> CreateLabOrder(
            [FromBody] CreateLabOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var order = await _labOrderService.CreateLabOrderAsync(request);
                return CreatedAtAction(
                    nameof(GetLabOrder),
                    new { id = order.Id },
                    order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lab order");
                return StatusCode(500, new { 
                    Message = "An error occurred while creating the lab order",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Cancel lab order
        /// </summary>
        [HttpPost("{id}/cancel")]
        //[Authorize(Roles = "Patient,Laboratory,Admin")]
        [ProducesResponseType(typeof(LabOrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LabOrderResponse>> CancelLabOrder(
            Guid id,
            [FromBody] CancelLabOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var order = await _labOrderService.CancelLabOrderAsync(id, request.CancellationReason);
                return Ok(order);
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
                _logger.LogError(ex, "Error cancelling lab order {OrderId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Delete lab order (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteLabOrder(Guid id)
        {
            try
            {
                var result = await _labOrderService.DeleteLabOrderAsync(id);
                if (!result)
                    return NotFound(new { Message = $"Lab order with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lab order {OrderId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        // ==================== Order Lifecycle ====================

        /// <summary>
        /// Confirm lab order by laboratory
        /// </summary>
        [HttpPost("{id}/confirm")]
        //[Authorize(Roles = "Laboratory")]
        [ProducesResponseType(typeof(LabOrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LabOrderResponse>> ConfirmLabOrder(Guid id)
        {
            try
            {
                var order = await _labOrderService.ConfirmLabOrderAsync(id);
                return Ok(order);
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
                _logger.LogError(ex, "Error confirming lab order {OrderId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Mark order as sample collected
        /// </summary>
        [HttpPost("{id}/sample-collected")]
        //[Authorize(Roles = "Laboratory")]
        [ProducesResponseType(typeof(LabOrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LabOrderResponse>> MarkSampleCollected(Guid id)
        {
            try
            {
                var order = await _labOrderService.MarkSampleCollectedAsync(id);
                return Ok(order);
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
                _logger.LogError(ex, "Error marking sample collected for lab order {OrderId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Mark order as in progress (tests being performed)
        /// </summary>
        [HttpPost("{id}/in-progress")]
        //[Authorize(Roles = "Laboratory")]
        [ProducesResponseType(typeof(LabOrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LabOrderResponse>> MarkInProgress(Guid id)
        {
            try
            {
                var order = await _labOrderService.MarkInProgressAsync(id);
                return Ok(order);
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
                _logger.LogError(ex, "Error marking lab order {OrderId} as in progress", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Complete lab order (all results ready)
        /// </summary>
        [HttpPost("{id}/complete")]
        //[Authorize(Roles = "Laboratory")]
        [ProducesResponseType(typeof(LabOrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LabOrderResponse>> CompleteLabOrder(Guid id)
        {
            try
            {
                var order = await _labOrderService.CompleteLabOrderAsync(id);
                return Ok(order);
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
                _logger.LogError(ex, "Error completing lab order {OrderId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Mark lab order as paid
        /// </summary>
        [HttpPost("{id}/mark-paid")]
        //[Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(typeof(LabOrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LabOrderResponse>> MarkLabOrderAsPaid(
            Guid id,
            [FromBody] MarkAsPaidRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var order = await _labOrderService.MarkLabOrderAsPaidAsync(id, request.PaymentMethod, request.TransactionId);
                return Ok(order);
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
                _logger.LogError(ex, "Error marking lab order {OrderId} as paid", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        // ==================== Results Management ====================

        /// <summary>
        /// Get lab order results
        /// </summary>
        [HttpGet("{id}/results")]
        //[Authorize(Roles = "Patient,Doctor,Laboratory,Admin")]
        [ProducesResponseType(typeof(IEnumerable<LabResultResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LabResultResponse>>> GetLabOrderResults(Guid id)
        {
            try
            {
                var results = await _labOrderService.GetLabOrderResultsAsync(id);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting results for lab order {OrderId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Add result to lab order
        /// </summary>
        [HttpPost("{id}/results")]
        //[Authorize(Roles = "Laboratory")]
        [ProducesResponseType(typeof(LabResultResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LabResultResponse>> AddLabOrderResult(
            Guid id,
            [FromBody] CreateLabResultRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _labOrderService.AddLabOrderResultAsync(id, request);
                return CreatedAtAction(
                    nameof(GetLabOrderResults),
                    new { id },
                    result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding result to lab order {OrderId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Update lab result
        /// </summary>
        [HttpPut("results/{resultId}")]
        //[Authorize(Roles = "Laboratory")]
        [ProducesResponseType(typeof(LabResultResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LabResultResponse>> UpdateLabResult(
            Guid resultId,
            [FromBody] UpdateLabResultRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _labOrderService.UpdateLabResultAsync(resultId, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lab result {ResultId}", resultId);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        // ==================== Statistics ====================

        /// <summary>
        /// Get lab order statistics
        /// </summary>
        [HttpGet("statistics")]
        //[Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(typeof(LabOrderStatistics), StatusCodes.Status200OK)]
        public async Task<ActionResult<LabOrderStatistics>> GetLabOrderStatistics(
            [FromQuery] Guid? laboratoryId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _labOrderService.GetLabOrderStatisticsAsync(laboratoryId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab order statistics");
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }
    }

    // Helper request models
    public class CancelLabOrderRequest
    {
        public string CancellationReason { get; set; } = string.Empty;
    }

    public class MarkAsPaidRequest
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
    }
}
