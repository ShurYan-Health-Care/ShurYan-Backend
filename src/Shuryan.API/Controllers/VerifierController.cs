using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests;
using Shuryan.Application.DTOs.Responses;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerifierController : ControllerBase
    {
        private readonly IVerifierService _verifierService;
        private readonly ILogger<VerifierController> _logger;

        public VerifierController(IVerifierService verifierService, ILogger<VerifierController> logger)
        {
            _verifierService = verifierService;
            _logger = logger;
        }

        #region Doctor Verification Status Management

        [HttpPost("doctors/{doctorId}/start-review")]
        [Authorize(Roles = "Verifier,Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> StartDoctorReview(Guid doctorId)
        {
            _logger.LogInformation("Request to start review for doctor {DoctorId}", doctorId);

            try
            {
                var result = await _verifierService.StartDoctorReviewAsync(doctorId);
                return Ok(ApiResponse<object>.Success(new { updated = result }, "Review has been started"));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting review for doctor: {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred", new[] { ex.Message }, 500));
            }
        }

        [HttpPost("doctors/{doctorId}/verify")]
        [Authorize(Roles = "Verifier,Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> VerifyDoctor(Guid doctorId)
        {
            var currentVerifierId = GetCurrentUserId();
            
            if (currentVerifierId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid or missing authentication token", statusCode: 401));
            }

            _logger.LogInformation("Request to verify doctor {DoctorId} by verifier {VerifierId}", doctorId, currentVerifierId);

            try
            {
                var result = await _verifierService.VerifyDoctorAsync(doctorId, currentVerifierId);
                return Ok(ApiResponse<object>.Success(new { updated = result }, "Doctor has been verified"));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying doctor: {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred", new[] { ex.Message }, 500));
            }
        }

        [HttpPost("doctors/{doctorId}/reject")]
        [Authorize(Roles = "Verifier,Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> RejectDoctor(Guid doctorId)
        {
            _logger.LogInformation("Request to reject doctor {DoctorId}", doctorId);

            try
            {
                var result = await _verifierService.RejectDoctorAsync(doctorId);
                return Ok(ApiResponse<object>.Success(new { updated = result }, "Doctor has been rejected"));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting doctor: {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred", new[] { ex.Message }, 500));
            }
        }

        #endregion

        #region Get Doctors by Verification Status

        [HttpGet("doctors/status/sent")]
        [Authorize(Roles = "Verifier,Admin")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DoctorVerificationListResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DoctorVerificationListResponse>>>> GetDoctorsWithSentStatus(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Request to get doctors with Sent status. Page: {Page}, Size: {Size}", pageNumber, pageSize);

            try
            {
                var paginationParams = new PaginationParams
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _verifierService.GetDoctorsWithSentStatusAsync(paginationParams);

                _logger.LogInformation("Successfully retrieved {Count} doctors with Sent status out of {Total}",
                    result.Data.Count(), result.TotalCount);

                return Ok(ApiResponse<PaginatedResponse<DoctorVerificationListResponse>>.Success(
                    result,
                    "Doctors with Sent status retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctors with Sent status");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving doctors",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("doctors/status/under-review")]
        [Authorize(Roles = "Verifier,Admin")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DoctorVerificationListResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DoctorVerificationListResponse>>>> GetDoctorsUnderReview(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Request to get doctors under review. Page: {Page}, Size: {Size}", pageNumber, pageSize);

            try
            {
                var paginationParams = new PaginationParams
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _verifierService.GetDoctorsUnderReviewAsync(paginationParams);

                _logger.LogInformation("Successfully retrieved {Count} doctors under review out of {Total}",
                    result.Data.Count(), result.TotalCount);

                return Ok(ApiResponse<PaginatedResponse<DoctorVerificationListResponse>>.Success(
                    result,
                    "Doctors under review retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctors under review");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving doctors",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("doctors/status/verified")]
        [Authorize(Roles = "Verifier,Admin")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DoctorVerificationListResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DoctorVerificationListResponse>>>> GetVerifiedDoctors(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var currentVerifierId = GetCurrentUserId();
            
            if (currentVerifierId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid or missing authentication token", statusCode: 401));
            }

            _logger.LogInformation("Request to get verified doctors by verifier {VerifierId}. Page: {Page}, Size: {Size}", 
                currentVerifierId, pageNumber, pageSize);

            try
            {
                var paginationParams = new PaginationParams
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _verifierService.GetVerifiedDoctorsAsync(paginationParams, currentVerifierId);

                _logger.LogInformation("Successfully retrieved {Count} verified doctors by verifier {VerifierId} out of {Total}",
                    result.Data.Count(), currentVerifierId, result.TotalCount);

                return Ok(ApiResponse<PaginatedResponse<DoctorVerificationListResponse>>.Success(
                    result,
                    "Verified doctors retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving verified doctors for verifier {VerifierId}", currentVerifierId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving doctors",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("doctors/status/rejected")]
        [Authorize(Roles = "Verifier,Admin")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DoctorVerificationListResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DoctorVerificationListResponse>>>> GetRejectedDoctors(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Request to get rejected doctors. Page: {Page}, Size: {Size}", pageNumber, pageSize);

            try
            {
                var paginationParams = new PaginationParams
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _verifierService.GetRejectedDoctorsAsync(paginationParams);

                _logger.LogInformation("Successfully retrieved {Count} rejected doctors out of {Total}",
                    result.Data.Count(), result.TotalCount);

                return Ok(ApiResponse<PaginatedResponse<DoctorVerificationListResponse>>.Success(
                    result,
                    "Rejected doctors retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rejected doctors");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving doctors",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Document Verification

        /// <summary>
        /// جلب مستندات دكتور معين
        /// </summary>
        [HttpGet("doctors/{doctorId}/documents")]
        [Authorize(Roles = "Verifier,Admin")]
        [ProducesResponseType(typeof(ApiResponse<List<DoctorDocumentItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<DoctorDocumentItemResponse>>>> GetDoctorDocuments(Guid doctorId)
        {
            _logger.LogInformation("Request to get documents for doctor {DoctorId}", doctorId);

            try
            {
                var documents = await _verifierService.GetDoctorDocumentsAsync(doctorId);
                return Ok(ApiResponse<List<DoctorDocumentItemResponse>>.Success(
                    documents,
                    "Doctor documents retrieved successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents for doctor: {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred", new[] { ex.Message }, 500));
            }
        }

        /// <summary>
        /// قبول مستند الدكتور
        /// </summary>
        [HttpPost("documents/{documentId}/approve")]
        [Authorize(Roles = "Verifier,Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> ApproveDocument(Guid documentId)
        {
            _logger.LogInformation("Request to approve document {DocumentId}", documentId);

            try
            {
                var result = await _verifierService.ApproveDocumentAsync(documentId);
                return Ok(ApiResponse<object>.Success(new { updated = result }, "Document has been approved"));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving document: {DocumentId}", documentId);
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred", new[] { ex.Message }, 500));
            }
        }

        /// <summary>
        /// رفض مستند الدكتور مع سبب الرفض (اختياري)
        /// </summary>
        [HttpPost("documents/{documentId}/reject")]
        [Authorize(Roles = "Verifier,Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> RejectDocument(
            Guid documentId,
            [FromBody] Shuryan.Application.DTOs.Requests.Verifier.RejectDocumentRequest request)
        {
            _logger.LogInformation("Request to reject document {DocumentId} with reason: {Reason}", 
                documentId, request?.RejectionReason ?? "No reason provided");

            try
            {
                var result = await _verifierService.RejectDocumentAsync(documentId, request?.RejectionReason);
                return Ok(ApiResponse<object>.Success(new { updated = result }, "Document has been rejected"));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting document: {DocumentId}", documentId);
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred", new[] { ex.Message }, 500));
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// جلب الـ User ID الحالي من الـ JWT token
        /// </summary>
        private Guid GetCurrentUserId()
        {
            // Try multiple claim types that might contain the user ID
            var userIdClaim = User.Claims.FirstOrDefault(c => 
                c.Type == "sub" || 
                c.Type == "uid" || 
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogDebug("Found user ID in claim: {ClaimType} = {UserId}", userIdClaim.Type, userId);
                return userId;
            }

            _logger.LogWarning("Could not find user ID in JWT claims. Available claims: {Claims}", 
                string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
            
            return Guid.Empty;
        }

        #endregion
    }
}
