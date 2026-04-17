using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Review;
using Shuryan.Application.DTOs.Responses.Review;
using Shuryan.Application.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/Doctors/me/reviews")]
    [Authorize(Roles = "Doctor")]
    public class DoctorReviewsController : ControllerBase
    {
        private readonly IDoctorReviewService _reviewService;
        private readonly ILogger<DoctorReviewsController> _logger;

        public DoctorReviewsController(
            IDoctorReviewService reviewService,
            ILogger<DoctorReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        #region Review Operations

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DoctorReviewListItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DoctorReviewListItemResponse>>>> GetReviews(
            [FromQuery] PaginationParams paginationParams)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access doctor reviews - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid pagination parameters for doctor reviews");
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid pagination parameters",
                    statusCode: 400
                ));
            }

            _logger.LogInformation("Get doctor reviews request for doctor: {DoctorId}, Page: {Page}, PageSize: {PageSize}", 
                currentDoctorId, paginationParams.PageNumber, paginationParams.PageSize);

            try
            {
                var result = await _reviewService.GetDoctorReviewsAsync(currentDoctorId, paginationParams);
                return Ok(ApiResponse<PaginatedResponse<DoctorReviewListItemResponse>>.Success(
                    result,
                    "Reviews retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving reviews",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponse<DoctorReviewStatisticsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<DoctorReviewStatisticsResponse>>> GetStatistics()
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access review statistics - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get review statistics request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var result = await _reviewService.GetReviewStatisticsAsync(currentDoctorId);
                return Ok(ApiResponse<DoctorReviewStatisticsResponse>.Success(
                    result,
                    "Review statistics retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review statistics for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving review statistics",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(ApiResponse<DoctorReviewDetailsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DoctorReviewDetailsResponse>>> GetDetails(Guid id)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access review details - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get review details request for review: {ReviewId}, doctor: {DoctorId}", id, currentDoctorId);

            try
            {
                var result = await _reviewService.GetReviewDetailsAsync(id, currentDoctorId);
                
                if (result == null)
                {
                    _logger.LogWarning("Review {ReviewId} not found for doctor {DoctorId}", id, currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Review with ID {id} not found",
                        statusCode: 404
                    ));
                }

                return Ok(ApiResponse<DoctorReviewDetailsResponse>.Success(
                    result,
                    "Review details retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review details {ReviewId} for doctor: {DoctorId}", id, currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving review details",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("{reviewId}/reply")]
        [ProducesResponseType(typeof(ApiResponse<DoctorReviewDetailsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DoctorReviewDetailsResponse>>> ReplyToReview(
            Guid reviewId,
            [FromBody] ReplyToReviewRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (currentDoctorId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to reply to review - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid request body for replying to review");
                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid request data",
                    statusCode: 400
                ));
            }

            _logger.LogInformation("Reply to review request for review: {ReviewId}, doctor: {DoctorId}", reviewId, currentDoctorId);

            try
            {
                var result = await _reviewService.ReplyToReviewAsync(reviewId, currentDoctorId, request);
                return Ok(ApiResponse<DoctorReviewDetailsResponse>.Success(
                    result,
                    "Reply added successfully"
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Review {ReviewId} not found for doctor {DoctorId}", reviewId, currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replying to review {ReviewId} for doctor: {DoctorId}", reviewId, currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while replying to review",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Helper Methods

        private Guid GetCurrentDoctorId()
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

            _logger.LogWarning("Could not find doctor ID in JWT claims. Available claims: {Claims}", 
                string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
            
            return Guid.Empty;
        }

        #endregion
    }
}

