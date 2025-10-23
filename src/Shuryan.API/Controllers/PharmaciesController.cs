using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Pharmacy;
using Shuryan.Application.DTOs.Responses.Pharmacy;
using Shuryan.Application.DTOs.Responses.Review;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Enums;
using System.Security.Claims;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PharmaciesController : ControllerBase
    {
        private readonly IPharmacyService _pharmacyService;
        private readonly ILogger<PharmaciesController> _logger;

        public PharmaciesController(IPharmacyService pharmacyService, ILogger<PharmaciesController> logger)
        {
            _pharmacyService = pharmacyService;
            _logger = logger;
        }

        #region Helper Methods
        private Guid GetCurrentPharmacyId()
        {
            var pharmacyIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(pharmacyIdClaim) || !Guid.TryParse(pharmacyIdClaim, out var pharmacyId))
            {
                return Guid.Empty;
            }
            return pharmacyId;
        }

        private bool IsAccessingOwnData(Guid pharmacyId)
        {
            // Admins and Verifiers can access any pharmacy's data
            if (User.IsInRole("Admin") || User.IsInRole("Verifier"))
            {
                return true;
            }

            var currentPharmacyId = GetCurrentPharmacyId();
            return currentPharmacyId != Guid.Empty && currentPharmacyId == pharmacyId;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }
        #endregion

        #region Profile Management
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyResponse>>> GetPharmacy(Guid id)
        {
            _logger.LogInformation("Get pharmacy request for ID: {PharmacyId}", id);

            try
            {
                var pharmacy = await _pharmacyService.GetPharmacyByIdAsync(id);
                if (pharmacy == null)
                {
                    _logger.LogWarning("Pharmacy not found: {PharmacyId}", id);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Pharmacy with ID {id} not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Pharmacy retrieved successfully: {PharmacyId}", id);
                return Ok(ApiResponse<PharmacyResponse>.Success(
                    pharmacy,
                    "Pharmacy retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pharmacy: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving pharmacy",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("email/{email}")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyResponse>>> GetPharmacyByEmail(string email)
        {
            _logger.LogInformation("Get pharmacy by email request: {Email}", email);

            try
            {
                var pharmacy = await _pharmacyService.GetPharmacyByEmailAsync(email);
                if (pharmacy == null)
                {
                    _logger.LogWarning("Pharmacy not found with email: {Email}", email);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Pharmacy with email {email} not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Pharmacy retrieved successfully by email: {Email}", email);
                return Ok(ApiResponse<PharmacyResponse>.Success(
                    pharmacy,
                    "Pharmacy retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pharmacy by email: {Email}", email);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving pharmacy",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeletePharmacy(Guid id)
        {
            _logger.LogInformation("Delete pharmacy request for ID: {PharmacyId}", id);

            try
            {
                var result = await _pharmacyService.DeletePharmacyAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Pharmacy not found for deletion: {PharmacyId}", id);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Pharmacy with ID {id} not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Pharmacy deleted successfully: {PharmacyId}", id);
                return Ok(ApiResponse<object>.Success(
                    null,
                    "Pharmacy deleted successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting pharmacy: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while deleting pharmacy",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("me")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyResponse>>> GetCurrentPharmacy()
        {
            var pharmacyIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(pharmacyIdClaim) || !Guid.TryParse(pharmacyIdClaim, out var pharmacyId))
            {
                _logger.LogWarning("Unauthorized attempt to access pharmacy profile");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get current pharmacy profile request: {PharmacyId}", pharmacyId);

            try
            {
                var pharmacy = await _pharmacyService.GetCurrentPharmacyAsync(pharmacyId);
                if (pharmacy == null)
                {
                    _logger.LogWarning("Pharmacy profile not found: {PharmacyId}", pharmacyId);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Pharmacy with ID {pharmacyId} not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Pharmacy profile retrieved successfully: {PharmacyId}", pharmacyId);
                return Ok(ApiResponse<PharmacyResponse>.Success(
                    pharmacy,
                    "Profile retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pharmacy profile: {PharmacyId}", pharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving profile",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("me")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyResponse>>> UpdateCurrentPharmacy([FromBody] UpdatePharmacyRequest request)
        {
            var pharmacyIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(pharmacyIdClaim) || !Guid.TryParse(pharmacyIdClaim, out var pharmacyId))
            {
                _logger.LogWarning("Unauthorized attempt to update pharmacy profile");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update pharmacy request for: {PharmacyId}", pharmacyId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "Validation failed",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Update pharmacy profile request: {PharmacyId}", pharmacyId);

            try
            {
                var pharmacy = await _pharmacyService.UpdatePharmacyAsync(pharmacyId, request);
                _logger.LogInformation("Pharmacy profile updated successfully: {PharmacyId}", pharmacyId);
                return Ok(ApiResponse<PharmacyResponse>.Success(
                    pharmacy,
                    "Profile updated successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Pharmacy not found for update: {PharmacyId}", pharmacyId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pharmacy profile: {PharmacyId}", pharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating profile",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Working Hours Management
        [HttpGet("{id}/working-hours")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PharmacyWorkingHoursResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PharmacyWorkingHoursResponse>>>> GetWorkingHours(Guid id)
        {
            _logger.LogInformation("Get working hours request for pharmacy: {PharmacyId}", id);

            try
            {
                var hours = await _pharmacyService.GetWorkingHoursAsync(id);
                _logger.LogInformation("Retrieved {Count} working hours for pharmacy: {PharmacyId}", hours.Count(), id);
                return Ok(ApiResponse<IEnumerable<PharmacyWorkingHoursResponse>>.Success(
                    hours,
                    $"Retrieved {hours.Count()} working hours successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving working hours for pharmacy: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving working hours",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("{id}/working-hours")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyWorkingHoursResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyWorkingHoursResponse>>> AddWorkingHours(Guid id, [FromBody] CreatePharmacyWorkingHoursRequest request)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to add working hours for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to modify this pharmacy's working hours",
                    statusCode: 403
                ));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid add working hours request for pharmacy: {PharmacyId}", id);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Add working hours request for pharmacy: {PharmacyId}", id);

            try
            {
                var hours = await _pharmacyService.AddWorkingHoursAsync(id, request);
                _logger.LogInformation("Working hours added successfully for pharmacy: {PharmacyId}", id);
                return CreatedAtAction(
                    nameof(GetWorkingHours),
                    new { id },
                    ApiResponse<PharmacyWorkingHoursResponse>.Success(
                        hours,
                        "Working hours added successfully",
                        201
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding working hours for pharmacy: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while adding working hours",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("{id}/working-hours/{workingHoursId}")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyWorkingHoursResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyWorkingHoursResponse>>> UpdateWorkingHours(Guid id, Guid workingHoursId, [FromBody] UpdatePharmacyWorkingHoursRequest request)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to update working hours for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to modify this pharmacy's working hours",
                    statusCode: 403
                ));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update working hours request for pharmacy: {PharmacyId}", id);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Update working hours request for pharmacy: {PharmacyId}, WorkingHoursId: {WorkingHoursId}", id, workingHoursId);

            try
            {
                var hours = await _pharmacyService.UpdateWorkingHoursAsync(id, workingHoursId, request);
                _logger.LogInformation("Working hours updated successfully for pharmacy: {PharmacyId}", id);
                return Ok(ApiResponse<PharmacyWorkingHoursResponse>.Success(
                    hours,
                    "Working hours updated successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Working hours not found for pharmacy: {PharmacyId}, WorkingHoursId: {WorkingHoursId}", id, workingHoursId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating working hours for pharmacy: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating working hours",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpDelete("{id}/working-hours/{workingHoursId}")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteWorkingHours(Guid id, Guid workingHoursId)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to delete working hours for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to modify this pharmacy's working hours",
                    statusCode: 403
                ));
            }

            _logger.LogInformation("Delete working hours request for pharmacy: {PharmacyId}, WorkingHoursId: {WorkingHoursId}", id, workingHoursId);

            try
            {
                var result = await _pharmacyService.DeleteWorkingHoursAsync(id, workingHoursId);
                if (!result)
                {
                    _logger.LogWarning("Working hours not found for deletion: {PharmacyId}, WorkingHoursId: {WorkingHoursId}", id, workingHoursId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Working hours not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Working hours deleted successfully for pharmacy: {PharmacyId}", id);
                return Ok(ApiResponse<object>.Success(
                    null,
                    "Working hours deleted successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting working hours for pharmacy: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while deleting working hours",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{id}/is-open")]
        [ProducesResponseType(typeof(ApiResponse<IsOpenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IsOpenResponse>>> IsPharmacyOpen(Guid id)
        {
            _logger.LogInformation("Check if pharmacy is open request: {PharmacyId}", id);

            try
            {
                var response = await _pharmacyService.IsPharmacyOpenAsync(id);
                _logger.LogInformation("Pharmacy open status checked: {PharmacyId}, IsOpen: {IsOpen}", id, response.IsOpen);
                return Ok(ApiResponse<IsOpenResponse>.Success(
                    response,
                    response.IsOpen ? "Pharmacy is currently open" : "Pharmacy is currently closed"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking pharmacy open status: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while checking pharmacy status",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Orders Management
        [HttpGet("{id}/orders")]
        [Authorize(Roles = "Pharmacy,Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PharmacyOrderResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PharmacyOrderResponse>>>> GetPharmacyOrders(Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to access orders for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to access this pharmacy's orders",
                    statusCode: 403
                ));
            }

            _logger.LogInformation("Get pharmacy orders request: {PharmacyId}, Page: {PageNumber}, Size: {PageSize}", id, pageNumber, pageSize);

            try
            {
                var orders = await _pharmacyService.GetPharmacyOrdersAsync(id, pageNumber, pageSize);
                _logger.LogInformation("Retrieved {Count} orders for pharmacy: {PharmacyId}", orders.Count(), id);
                return Ok(ApiResponse<IEnumerable<PharmacyOrderResponse>>.Success(
                    orders,
                    $"Retrieved {orders.Count()} orders successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for pharmacy: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving orders",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{id}/orders/{orderId}")]
        [Authorize(Roles = "Pharmacy,Admin")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyOrderResponse>>> GetOrderById(Guid id, Guid orderId)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to access order for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to access this pharmacy's orders",
                    statusCode: 403
                ));
            }

            _logger.LogInformation("Get order by ID request: {PharmacyId}, OrderId: {OrderId}", id, orderId);

            try
            {
                var order = await _pharmacyService.GetOrderByIdAsync(id, orderId);
                if (order == null)
                {
                    _logger.LogWarning("Order not found: {PharmacyId}, OrderId: {OrderId}", id, orderId);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Order with ID {orderId} not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Order retrieved successfully: {OrderId}", orderId);
                return Ok(ApiResponse<PharmacyOrderResponse>.Success(
                    order,
                    "Order retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order: {PharmacyId}, OrderId: {OrderId}", id, orderId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving order",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("{id}/orders/{orderId}/status")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyOrderResponse>>> UpdateOrderStatus(Guid id, Guid orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to update order status for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to modify this pharmacy's orders",
                    statusCode: 403
                ));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update order status request: {PharmacyId}, OrderId: {OrderId}", id, orderId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Update order status request: {PharmacyId}, OrderId: {OrderId}", id, orderId);

            try
            {
                var order = await _pharmacyService.UpdateOrderStatusAsync(id, orderId, request);
                _logger.LogInformation("Order status updated successfully: {OrderId}", orderId);
                return Ok(ApiResponse<PharmacyOrderResponse>.Success(
                    order,
                    "Order status updated successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Order not found for status update: {OrderId}", orderId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status: {OrderId}", orderId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating order status",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{id}/orders/pending")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PharmacyOrderResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PharmacyOrderResponse>>>> GetPendingOrders(Guid id)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to access pending orders for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to access this pharmacy's orders",
                    statusCode: 403
                ));
            }

            _logger.LogInformation("Get pending orders request for pharmacy: {PharmacyId}", id);

            try
            {
                var orders = await _pharmacyService.GetPendingOrdersAsync(id);
                _logger.LogInformation("Retrieved {Count} pending orders for pharmacy: {PharmacyId}", orders.Count(), id);
                return Ok(ApiResponse<IEnumerable<PharmacyOrderResponse>>.Success(
                    orders,
                    $"Retrieved {orders.Count()} pending orders successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending orders for pharmacy: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving pending orders",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{id}/orders/in-progress")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PharmacyOrderResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PharmacyOrderResponse>>>> GetInProgressOrders(Guid id)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to access in-progress orders for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to access this pharmacy's orders",
                    statusCode: 403
                ));
            }

            _logger.LogInformation("Get in-progress orders request for pharmacy: {PharmacyId}", id);

            try
            {
                var orders = await _pharmacyService.GetInProgressOrdersAsync(id);
                _logger.LogInformation("Retrieved {Count} in-progress orders for pharmacy: {PharmacyId}", orders.Count(), id);
                return Ok(ApiResponse<IEnumerable<PharmacyOrderResponse>>.Success(
                    orders,
                    $"Retrieved {orders.Count()} in-progress orders successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving in-progress orders for pharmacy: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving in-progress orders",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{id}/orders/completed")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PharmacyOrderResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PharmacyOrderResponse>>>> GetCompletedOrders(Guid id)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to access completed orders for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to access this pharmacy's orders",
                    statusCode: 403
                ));
            }

            _logger.LogInformation("Get completed orders request for pharmacy: {PharmacyId}", id);

            try
            {
                var orders = await _pharmacyService.GetCompletedOrdersAsync(id);
                _logger.LogInformation("Retrieved {Count} completed orders for pharmacy: {PharmacyId}", orders.Count(), id);
                return Ok(ApiResponse<IEnumerable<PharmacyOrderResponse>>.Success(
                    orders,
                    $"Retrieved {orders.Count()} completed orders successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving completed orders for pharmacy: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving completed orders",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("{id}/orders/{orderId}/accept")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyOrderResponse>>> AcceptOrder(Guid id, Guid orderId)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to accept order for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to modify this pharmacy's orders",
                    statusCode: 403
                ));
            }

            _logger.LogInformation("Accept order request: {PharmacyId}, OrderId: {OrderId}", id, orderId);

            try
            {
                var order = await _pharmacyService.AcceptOrderAsync(id, orderId);
                _logger.LogInformation("Order accepted successfully: {OrderId}", orderId);
                return Ok(ApiResponse<PharmacyOrderResponse>.Success(
                    order,
                    "Order accepted successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Order not found for acceptance: {OrderId}", orderId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting order: {OrderId}", orderId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while accepting order",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("{id}/orders/{orderId}/reject")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyOrderResponse>>> RejectOrder(Guid id, Guid orderId, [FromBody] RejectOrderRequest request)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to reject order for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to modify this pharmacy's orders",
                    statusCode: 403
                ));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid reject order request: {PharmacyId}, OrderId: {OrderId}", id, orderId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Reject order request: {PharmacyId}, OrderId: {OrderId}", id, orderId);

            try
            {
                var order = await _pharmacyService.RejectOrderAsync(id, orderId, request);
                _logger.LogInformation("Order rejected successfully: {OrderId}", orderId);
                return Ok(ApiResponse<PharmacyOrderResponse>.Success(
                    order,
                    "Order rejected successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Order not found for rejection: {OrderId}", orderId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting order: {OrderId}", orderId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while rejecting order",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("{id}/orders/{orderId}/prepare")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyOrderResponse>>> MarkOrderAsPreparing(Guid id, Guid orderId)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to prepare order for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to modify this pharmacy's orders",
                    statusCode: 403
                ));
            }

            _logger.LogInformation("Mark order as preparing request: {PharmacyId}, OrderId: {OrderId}", id, orderId);

            try
            {
                var order = await _pharmacyService.MarkOrderAsPreparingAsync(id, orderId);
                _logger.LogInformation("Order marked as preparing: {OrderId}", orderId);
                return Ok(ApiResponse<PharmacyOrderResponse>.Success(
                    order,
                    "Order marked as preparing successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", orderId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order as preparing: {OrderId}", orderId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating order",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("{id}/orders/{orderId}/ready")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyOrderResponse>>> MarkOrderAsReady(Guid id, Guid orderId)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to mark order ready for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to modify this pharmacy's orders",
                    statusCode: 403
                ));
            }

            _logger.LogInformation("Mark order as ready request: {PharmacyId}, OrderId: {OrderId}", id, orderId);

            try
            {
                var order = await _pharmacyService.MarkOrderAsReadyAsync(id, orderId);
                _logger.LogInformation("Order marked as ready: {OrderId}", orderId);
                return Ok(ApiResponse<PharmacyOrderResponse>.Success(
                    order,
                    "Order marked as ready successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", orderId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order as ready: {OrderId}", orderId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating order",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("{id}/orders/{orderId}/dispatch")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyOrderResponse>>> DispatchOrder(Guid id, Guid orderId)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to dispatch order for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to modify this pharmacy's orders",
                    statusCode: 403
                ));
            }

            _logger.LogInformation("Dispatch order request: {PharmacyId}, OrderId: {OrderId}", id, orderId);

            try
            {
                var order = await _pharmacyService.DispatchOrderAsync(id, orderId);
                _logger.LogInformation("Order dispatched successfully: {OrderId}", orderId);
                return Ok(ApiResponse<PharmacyOrderResponse>.Success(
                    order,
                    "Order dispatched successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", orderId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching order: {OrderId}", orderId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while dispatching order",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("{id}/orders/{orderId}/deliver")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyOrderResponse>>> MarkOrderAsDelivered(Guid id, Guid orderId)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to mark order delivered for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to modify this pharmacy's orders",
                    statusCode: 403
                ));
            }

            _logger.LogInformation("Mark order as delivered request: {PharmacyId}, OrderId: {OrderId}", id, orderId);

            try
            {
                var order = await _pharmacyService.MarkOrderAsDeliveredAsync(id, orderId);
                _logger.LogInformation("Order marked as delivered: {OrderId}", orderId);
                return Ok(ApiResponse<PharmacyOrderResponse>.Success(
                    order,
                    "Order marked as delivered successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", orderId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order as delivered: {OrderId}", orderId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while updating order",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Search & Discovery
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PharmacyResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PharmacyResponse>>>> SearchPharmacies([FromQuery] SearchPharmaciesRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid search pharmacies request");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Search pharmacies request");

            try
            {
                var pharmacies = await _pharmacyService.SearchPharmaciesAsync(request);
                _logger.LogInformation("Found {Count} pharmacies matching search criteria", pharmacies.Count());
                return Ok(ApiResponse<IEnumerable<PharmacyResponse>>.Success(
                    pharmacies,
                    $"Found {pharmacies.Count()} pharmacies"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching pharmacies");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while searching pharmacies",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("governorate/{governorate}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PharmacyResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PharmacyResponse>>>> GetPharmaciesByGovernorate(Governorate governorate)
        {
            _logger.LogInformation("Get pharmacies by governorate request: {Governorate}", governorate);

            try
            {
                var pharmacies = await _pharmacyService.GetPharmaciesByGovernorateAsync(governorate);
                _logger.LogInformation("Retrieved {Count} pharmacies in governorate: {Governorate}", pharmacies.Count(), governorate);
                return Ok(ApiResponse<IEnumerable<PharmacyResponse>>.Success(
                    pharmacies,
                    $"Retrieved {pharmacies.Count()} pharmacies in {governorate}"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pharmacies by governorate: {Governorate}", governorate);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving pharmacies",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("nearby")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PharmacyResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PharmacyResponse>>>> GetNearbyPharmacies([FromQuery] NearbyPharmaciesRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid nearby pharmacies request");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Get nearby pharmacies request: Lat={Latitude}, Lng={Longitude}", request.Latitude, request.Longitude);

            try
            {
                var pharmacies = await _pharmacyService.GetNearbyPharmaciesAsync(request);
                _logger.LogInformation("Found {Count} nearby pharmacies", pharmacies.Count());
                return Ok(ApiResponse<IEnumerable<PharmacyResponse>>.Success(
                    pharmacies,
                    $"Found {pharmacies.Count()} nearby pharmacies"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nearby pharmacies");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving nearby pharmacies",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Document Management
        [HttpGet("{id}/documents")]
        [Authorize(Roles = "Pharmacy,Admin,Verifier")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PharmacyDocumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PharmacyDocumentResponse>>>> GetPharmacyDocuments(Guid id)
        {
            // Validate own data access (Admin and Verifier can access any pharmacy's documents)
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to access documents for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to access this pharmacy's documents",
                    statusCode: 403
                ));
            }

            _logger.LogInformation("Get pharmacy documents request: {PharmacyId}", id);

            try
            {
                var documents = await _pharmacyService.GetPharmacyDocumentsAsync(id);
                _logger.LogInformation("Retrieved {Count} documents for pharmacy: {PharmacyId}", documents.Count(), id);
                return Ok(ApiResponse<IEnumerable<PharmacyDocumentResponse>>.Success(
                    documents,
                    $"Retrieved {documents.Count()} documents successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pharmacy documents: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving documents",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("{id}/documents")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyDocumentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyDocumentResponse>>> UploadDocument(Guid id, [FromBody] CreatePharmacyDocumentRequest request)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to upload document for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to upload documents for this pharmacy",
                    statusCode: 403
                ));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid upload document request for pharmacy: {PharmacyId}", id);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Upload document request for pharmacy: {PharmacyId}", id);

            try
            {
                var document = await _pharmacyService.UploadDocumentAsync(id, request);
                _logger.LogInformation("Document uploaded successfully for pharmacy: {PharmacyId}", id);
                return CreatedAtAction(
                    nameof(GetPharmacyDocuments),
                    new { id },
                    ApiResponse<PharmacyDocumentResponse>.Success(
                        document,
                        "Document uploaded successfully",
                        201
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for pharmacy: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while uploading document",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Reviews Management
        [HttpGet("{id}/reviews")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PharmacyReviewResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PharmacyReviewResponse>>>> GetPharmacyReviews(Guid id)
        {
            _logger.LogInformation("Get pharmacy reviews request: {PharmacyId}", id);

            try
            {
                var reviews = await _pharmacyService.GetPharmacyReviewsAsync(id);
                _logger.LogInformation("Retrieved {Count} reviews for pharmacy: {PharmacyId}", reviews.Count(), id);
                return Ok(ApiResponse<IEnumerable<PharmacyReviewResponse>>.Success(
                    reviews,
                    $"Retrieved {reviews.Count()} reviews successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pharmacy reviews: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving reviews",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{id}/reviews/statistics")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyReviewStatisticsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyReviewStatisticsResponse>>> GetReviewStatistics(Guid id)
        {
            _logger.LogInformation("Get review statistics request for pharmacy: {PharmacyId}", id);

            try
            {
                var statistics = await _pharmacyService.GetReviewStatisticsAsync(id);
                _logger.LogInformation("Review statistics retrieved for pharmacy: {PharmacyId}", id);
                return Ok(ApiResponse<PharmacyReviewStatisticsResponse>.Success(
                    statistics,
                    "Review statistics retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review statistics for pharmacy: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving review statistics",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Address Management
        [HttpGet("{id}/address")]
        [ProducesResponseType(typeof(ApiResponse<AddressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AddressResponse>>> GetPharmacyAddress(Guid id)
        {
            _logger.LogInformation("Get pharmacy address request: {PharmacyId}", id);

            try
            {
                var address = await _pharmacyService.GetPharmacyAddressAsync(id);
                if (address == null)
                {
                    _logger.LogWarning("Address not found for pharmacy: {PharmacyId}", id);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Address not found for pharmacy with ID {id}",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Address retrieved for pharmacy: {PharmacyId}", id);
                return Ok(ApiResponse<AddressResponse>.Success(
                    address,
                    "Address retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pharmacy address: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving address",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Prescription Handling
        [HttpGet("{id}/prescriptions/{prescriptionId}")]
        [Authorize(Roles = "Pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionDetailsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PrescriptionDetailsResponse>>> GetPrescriptionDetails(Guid id, Guid prescriptionId)
        {
            // Validate own data access
            if (!IsAccessingOwnData(id))
            {
                _logger.LogWarning("Pharmacy {CurrentId} attempted to access prescription for pharmacy {TargetId}",
                    GetCurrentPharmacyId(), id);
                return StatusCode(403, ApiResponse<object>.Failure(
                    "You are not authorized to access this pharmacy's prescriptions",
                    statusCode: 403
                ));
            }

            _logger.LogInformation("Get prescription details request: {PharmacyId}, PrescriptionId: {PrescriptionId}", id, prescriptionId);

            try
            {
                var prescription = await _pharmacyService.GetPrescriptionDetailsAsync(id, prescriptionId);
                if (prescription == null)
                {
                    _logger.LogWarning("Prescription not found: {PrescriptionId}", prescriptionId);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Prescription with ID {prescriptionId} not found",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Prescription details retrieved: {PrescriptionId}", prescriptionId);
                return Ok(ApiResponse<PrescriptionDetailsResponse>.Success(
                    prescription,
                    "Prescription details retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescription details: {PrescriptionId}", prescriptionId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving prescription details",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Verification Management
        [HttpGet("pending-verification")]
        [Authorize(Roles = "Admin,Verifier")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PharmacyResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PharmacyResponse>>>> GetPendingVerificationPharmacies()
        {
            _logger.LogInformation("Get pending verification pharmacies request");

            try
            {
                var pharmacies = await _pharmacyService.GetPendingVerificationPharmaciesAsync();
                _logger.LogInformation("Retrieved {Count} pharmacies pending verification", pharmacies.Count());
                return Ok(ApiResponse<IEnumerable<PharmacyResponse>>.Success(
                    pharmacies,
                    $"Retrieved {pharmacies.Count()} pharmacies pending verification"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending verification pharmacies");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving pending pharmacies",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Admin,Verifier")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyResponse>>> VerifyPharmacy(Guid id, [FromBody] VerifyPharmacyRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid verify pharmacy request: {PharmacyId}", id);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Verify pharmacy request: {PharmacyId}", id);

            try
            {
                var pharmacy = await _pharmacyService.VerifyPharmacyAsync(id, request);
                _logger.LogInformation("Pharmacy verified successfully: {PharmacyId}", id);
                return Ok(ApiResponse<PharmacyResponse>.Success(
                    pharmacy,
                    "Pharmacy verified successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Pharmacy not found for verification: {PharmacyId}", id);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying pharmacy: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while verifying pharmacy",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("{id}/reject-verification")]
        [Authorize(Roles = "Admin,Verifier")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyResponse>>> RejectVerification(Guid id, [FromBody] RejectVerificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid reject verification request: {PharmacyId}", id);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Reject verification request: {PharmacyId}", id);

            try
            {
                var pharmacy = await _pharmacyService.RejectVerificationAsync(id, request);
                _logger.LogInformation("Pharmacy verification rejected: {PharmacyId}", id);
                return Ok(ApiResponse<PharmacyResponse>.Success(
                    pharmacy,
                    "Pharmacy verification rejected"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Pharmacy not found: {PharmacyId}", id);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting verification: {PharmacyId}", id);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while rejecting verification",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("{id}/documents/{documentId}/approve")]
        [Authorize(Roles = "Admin,Verifier")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyDocumentResponse>>> ApproveDocument(Guid id, Guid documentId, [FromBody] ApproveDocumentRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid approve document request: {PharmacyId}, DocumentId: {DocumentId}", id, documentId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Approve document request: {PharmacyId}, DocumentId: {DocumentId}", id, documentId);

            try
            {
                var document = await _pharmacyService.ApproveDocumentAsync(id, documentId, request);
                _logger.LogInformation("Document approved successfully: {DocumentId}", documentId);
                return Ok(ApiResponse<PharmacyDocumentResponse>.Success(
                    document,
                    "Document approved successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Document not found: {DocumentId}", documentId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving document: {DocumentId}", documentId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while approving document",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("{id}/documents/{documentId}/reject")]
        [Authorize(Roles = "Admin,Verifier")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyDocumentResponse>>> RejectDocument(Guid id, Guid documentId, [FromBody] RejectDocumentRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid reject document request: {PharmacyId}, DocumentId: {DocumentId}", id, documentId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Reject document request: {PharmacyId}, DocumentId: {DocumentId}", id, documentId);

            try
            {
                var document = await _pharmacyService.RejectDocumentAsync(id, documentId, request);
                _logger.LogInformation("Document rejected successfully: {DocumentId}", documentId);
                return Ok(ApiResponse<PharmacyDocumentResponse>.Success(
                    document,
                    "Document rejected successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Document not found: {DocumentId}", documentId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting document: {DocumentId}", documentId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while rejecting document",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion
    }
}
