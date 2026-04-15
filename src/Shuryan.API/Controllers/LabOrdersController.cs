using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.Interfaces;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/lab-orders")]
    [Authorize(Roles = "Patient,Doctor")]
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

        /// <summary>
        /// عرض النتائج (للمريض والدكتور)
        /// GET /api/lab-orders/{orderId}/results
        /// </summary>
        [HttpGet("{orderId:guid}/results")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderResultsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LabOrderResultsResponse>>> GetLabOrderResults(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            if (userId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));
            }

            _logger.LogInformation("Get lab order results request for order {OrderId} by {Role} {UserId}", 
                orderId, userRole, userId);

            try
            {
                var results = await _labOrderService.GetLabOrderResultsWithAuthAsync(
                    orderId, 
                    userId, 
                    userRole, 
                    cancellationToken);

                return Ok(ApiResponse<LabOrderResultsResponse>.Success(results, "تم جلب النتائج بنجاح"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Lab order not found: {OrderId}", orderId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to order {OrderId} by {Role} {UserId}", 
                    orderId, userRole, userId);
                return StatusCode(StatusCodes.Status403Forbidden, 
                    ApiResponse<object>.Failure(ex.Message, statusCode: 403));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting results for order {OrderId}", orderId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب النتائج", 
                    new[] { ex.Message }, 
                    500));
            }
        }

        #region Helper Methods

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c =>
                c.Type == "sub" ||
                c.Type == "uid" ||
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return Guid.Empty;
        }

        private string GetCurrentUserRole()
        {
            if (User.IsInRole("Patient")) return "Patient";
            if (User.IsInRole("Doctor")) return "Doctor";
            
            // Fallback checking claims directly if IsInRole fails
            var roleClaim = User.Claims.FirstOrDefault(c => 
                c.Type == ClaimTypes.Role || 
                c.Type == "role" ||
                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                
            return roleClaim?.Value ?? "Unknown";
        }

        #endregion
    }
}
