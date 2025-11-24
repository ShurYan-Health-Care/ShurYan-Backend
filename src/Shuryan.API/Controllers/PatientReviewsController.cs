using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
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
    [Route("api/patients/me/reviews")]
    [Authorize(Roles = "Patient")]
    public class PatientReviewsController : ControllerBase
    {
        private readonly IDoctorReviewService _reviewService;
        private readonly ILogger<PatientReviewsController> _logger;

        public PatientReviewsController(
            IDoctorReviewService reviewService,
            ILogger<PatientReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// Create a review for a completed appointment
        /// </summary>
        /// <param name="request">Review details</param>
        /// <returns>Created review</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DoctorReviewResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DoctorReviewResponse>>> CreateReview(
            [FromBody] CreateDoctorReviewRequest request)
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to create review - invalid token");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                _logger.LogWarning("Invalid review request from patient {PatientId}. Errors: {Errors}",
                    currentPatientId, string.Join(", ", errors));

                return BadRequest(ApiResponse<object>.Failure(
                    "Invalid review data",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Create review request from patient {PatientId} for appointment {AppointmentId}",
                currentPatientId, request.AppointmentId);

            try
            {
                var result = await _reviewService.CreateReviewAsync(currentPatientId, request);

                _logger.LogInformation("Review {ReviewId} created successfully by patient {PatientId}",
                    result.Id, currentPatientId);

                return CreatedAtAction(
                    nameof(CreateReview),
                    new { id = result.Id },
                    ApiResponse<DoctorReviewResponse>.Success(
                        result,
                        "Review created successfully",
                        201
                    )
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Appointment not found for review creation by patient {PatientId}", currentPatientId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Patient {PatientId} attempted to review appointment they don't own", currentPatientId);
                return StatusCode(403, ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 403
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid review operation by patient {PatientId}", currentPatientId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for patient {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while creating the review",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #region Helper Methods

        private Guid GetCurrentPatientId()
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

            _logger.LogWarning("Could not find patient ID in JWT claims. Available claims: {Claims}",
                string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));

            return Guid.Empty;
        }

        #endregion
    }
}
