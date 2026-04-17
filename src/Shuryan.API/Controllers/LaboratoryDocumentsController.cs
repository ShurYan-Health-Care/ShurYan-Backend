//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using Shuryan.Application.DTOs.Common.Base;
//using Shuryan.Application.DTOs.Requests.Laboratory;
//using Shuryan.Application.DTOs.Requests.Pharmacy;
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
//    [Route("api/laboratories/{laboratoryId}/[controller]")]
//    [Authorize]
//    public class LaboratoryDocumentsController : ControllerBase
//    {
//        private readonly ILaboratoryDocumentService _documentService;
//        private readonly ILogger<LaboratoryDocumentsController> _logger;

//        public LaboratoryDocumentsController(
//            ILaboratoryDocumentService documentService,
//            ILogger<LaboratoryDocumentsController> logger)
//        {
//            _documentService = documentService;
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

//        private bool IsAccessingOwnData(Guid laboratoryId)
//        {
//            var currentUserId = GetCurrentUserId();
//            return currentUserId == laboratoryId;
//        }

//        private bool IsAdmin()
//        {
//            return User.IsInRole("Admin");
//        }
//        #endregion

//        #region Document Management
//        [HttpGet]
//        [Authorize(Roles = "Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LaboratoryDocumentResponse>>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<IEnumerable<LaboratoryDocumentResponse>>>> GetLaboratoryDocuments(Guid laboratoryId)
//        {
//            _logger.LogInformation("Get laboratory documents request for laboratory: {LaboratoryId}", laboratoryId);

//            // Ownership check: Laboratories can only view their own documents unless they're Admin
//            if (!IsAdmin() && !IsAccessingOwnData(laboratoryId))
//            {
//                _logger.LogWarning("Forbidden: Laboratory {CurrentUserId} attempted to get documents of another laboratory {LaboratoryId}", 
//                    GetCurrentUserId(), laboratoryId);
//                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure(
//                    "Laboratories can only view their own documents",
//                    statusCode: 403
//                ));
//            }

//            try
//            {
//                var documents = await _documentService.GetLaboratoryDocumentsAsync(laboratoryId);
//                _logger.LogInformation("Successfully retrieved {Count} documents for laboratory: {LaboratoryId}", documents.Count(), laboratoryId);
//                return Ok(ApiResponse<IEnumerable<LaboratoryDocumentResponse>>.Success(
//                    documents,
//                    "Documents retrieved successfully"
//                ));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error retrieving documents for laboratory {LaboratoryId}", laboratoryId);
//                return StatusCode(500, ApiResponse<object>.Failure(
//                    "An unexpected error occurred while retrieving documents",
//                    new[] { ex.Message },
//                    500
//                ));
//            }
//        }

//        [HttpGet("{id}")]
//        [Authorize(Roles = "Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<LaboratoryDocumentResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LaboratoryDocumentResponse>>> GetDocument(Guid laboratoryId, Guid id)
//        {
//            _logger.LogInformation("Attempting to get document {DocumentId} for laboratory {LaboratoryId}", id, laboratoryId);

//            try
//            {
//                var document = await _documentService.GetDocumentByIdAsync(id);
//                if (document == null)
//                {
//                    _logger.LogWarning("Document not found: {DocumentId}", id);
//                    return NotFound(ApiResponse<object>.Failure($"Document with ID {id} not found", statusCode: 404));
//                }

//                if (document.LaboratoryId != laboratoryId)
//                {
//                    _logger.LogWarning("Mismatch: Document {DocumentId} does not belong to laboratory {LaboratoryId}", id, laboratoryId);
//                    return BadRequest(ApiResponse<object>.Failure("Document does not belong to this laboratory", statusCode: 400));
//                }

//                var currentUserId = GetCurrentUserId();
//                if (User.IsInRole("Laboratory") && !IsAdmin() && currentUserId != laboratoryId)
//                {
//                    _logger.LogWarning("Forbidden: Laboratory user {CurrentUserId} attempted to get document {DocumentId} for another laboratory {LaboratoryId}", currentUserId, id, laboratoryId);
//                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("Laboratories can only view their own documents", statusCode: 403));
//                }

//                return Ok(ApiResponse<LaboratoryDocumentResponse>.Success(document, "Document retrieved successfully"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting document {DocumentId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while retrieving the document", new[] { ex.Message }, 500));
//            }
//        }

//        [HttpPost]
//        [Authorize(Roles = "Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<LaboratoryDocumentResponse>), StatusCodes.Status201Created)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LaboratoryDocumentResponse>>> UploadDocument(
//            Guid laboratoryId,
//            [FromBody] CreateLaboratoryDocumentRequest request)
//        {
//            _logger.LogInformation("Upload document request for laboratory: {LaboratoryId}", laboratoryId);

//            // Ownership check: Laboratories can only upload documents for their own profile unless they're Admin
//            if (!IsAdmin() && !IsAccessingOwnData(laboratoryId))
//            {
//                _logger.LogWarning("Forbidden: Laboratory {CurrentUserId} attempted to upload document for another laboratory {LaboratoryId}", 
//                    GetCurrentUserId(), laboratoryId);
//                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure(
//                    "Laboratories can only upload documents for their own profile",
//                    statusCode: 403
//                ));
//            }

//            if (!ModelState.IsValid)
//            {
//                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
//                _logger.LogWarning("Invalid model state for UploadDocument for laboratory: {LaboratoryId}. Errors: {Errors}", 
//                    laboratoryId, string.Join(", ", errors));
//                return BadRequest(ApiResponse<object>.Failure(
//                    "Invalid request data",
//                    errors,
//                    400
//                ));
//            }

//            try
//            {
//                var document = await _documentService.UploadDocumentAsync(laboratoryId, request);
//                _logger.LogInformation("Document {DocumentId} uploaded successfully for laboratory {LaboratoryId}", document.Id, laboratoryId);

//                var response = ApiResponse<LaboratoryDocumentResponse>.Success(
//                    document,
//                    "Document uploaded successfully",
//                    201
//                );
//                return CreatedAtAction(
//                    nameof(GetDocument),
//                    new { laboratoryId, id = document.Id },
//                    response);
//            }
//            catch (ArgumentException ex)
//            {
//                _logger.LogWarning(ex, "Bad request on document upload for laboratory {LaboratoryId}", laboratoryId);
//                return BadRequest(ApiResponse<object>.Failure(
//                    ex.Message,
//                    statusCode: 400
//                ));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error uploading document for laboratory {LaboratoryId}", laboratoryId);
//                return StatusCode(500, ApiResponse<object>.Failure(
//                    "An unexpected error occurred while uploading the document",
//                    new[] { ex.Message },
//                    500
//                ));
//            }
//        }

//        [HttpDelete("{id}")]
//        [Authorize(Roles = "Laboratory,Admin")]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<object>>> DeleteDocument(Guid laboratoryId, Guid id)
//        {
//            _logger.LogInformation("Attempting to delete document {DocumentId} for laboratory {LaboratoryId}", id, laboratoryId);

//            var currentUserId = GetCurrentUserId();
//            if (User.IsInRole("Laboratory") && !IsAdmin() && currentUserId != laboratoryId)
//            {
//                _logger.LogWarning("Forbidden: Laboratory user {CurrentUserId} attempted to delete document {DocumentId} from another laboratory {LaboratoryId}", currentUserId, id, laboratoryId);
//                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Failure("Laboratories can only delete their own documents", statusCode: 403));
//            }

//            try
//            {
//                var result = await _documentService.DeleteDocumentAsync(id);
//                if (!result)
//                {
//                    _logger.LogWarning("Document not found for deletion: {DocumentId}", id);
//                    return NotFound(ApiResponse<object>.Failure($"Document with ID {id} not found", statusCode: 404));
//                }

//                _logger.LogInformation("Document deleted successfully: {DocumentId}", id);
//                return Ok(ApiResponse<object>.Success(null, "Document deleted successfully"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error deleting document {DocumentId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while deleting the document", new[] { ex.Message }, 500));
//            }
//        }

//        #endregion

//        #region Document Verification
//        [HttpPost("{id}/approve")]
//        [Authorize(Roles = "Admin,Verifier")]
//        [ProducesResponseType(typeof(ApiResponse<LaboratoryDocumentResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LaboratoryDocumentResponse>>> ApproveDocument(Guid laboratoryId, Guid id)
//        {
//            _logger.LogInformation("Approve document request for document {DocumentId} by user {UserId}", id, GetCurrentUserId());
            
//            try
//            {
//                var document = await _documentService.ApproveDocumentAsync(id);
//                _logger.LogInformation("Document {DocumentId} approved successfully", id);
//                return Ok(ApiResponse<LaboratoryDocumentResponse>.Success(
//                    document,
//                    "Document approved successfully"
//                ));
//            }
//            catch (ArgumentException ex)
//            {
//                _logger.LogWarning(ex, "Document not found for approval: {DocumentId}", id);
//                return NotFound(ApiResponse<object>.Failure(
//                    ex.Message,
//                    statusCode: 404
//                ));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error approving document {DocumentId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure(
//                    "An unexpected error occurred while approving the document",
//                    new[] { ex.Message },
//                    500
//                ));
//            }
//        }

//        [HttpPost("{id}/reject")]
//        [Authorize(Roles = "Admin,Verifier")]
//        [ProducesResponseType(typeof(ApiResponse<LaboratoryDocumentResponse>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<LaboratoryDocumentResponse>>> RejectDocument(
//            Guid laboratoryId,
//            Guid id,
//            [FromBody] RejectDocumentRequest request)
//        {
//            _logger.LogInformation("Attempting to reject document {DocumentId} for laboratory {LaboratoryId} by user {UserId}", id, laboratoryId, GetCurrentUserId());

//            if (!ModelState.IsValid)
//            {
//                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
//                _logger.LogWarning("Invalid model state for RejectDocument: {DocumentId}. Errors: {Errors}", id, string.Join(", ", errors));
//                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
//            }

//            try
//            {
//                var document = await _documentService.RejectDocumentAsync(id, request.RejectionReason);
//                _logger.LogInformation("Document {DocumentId} rejected successfully", id);
//                return Ok(ApiResponse<LaboratoryDocumentResponse>.Success(document, "Document rejected successfully"));
//            }
//            catch (ArgumentException ex)
//            {
//                _logger.LogWarning(ex, "Document not found for rejection: {DocumentId}", id);
//                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error rejecting document {DocumentId}", id);
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while rejecting the document", new[] { ex.Message }, 500));
//            }
//        }

//        [HttpGet("~/api/laboratory-documents/pending")]
//        [Authorize(Roles = "Admin,Verifier")]
//        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LaboratoryDocumentResponse>>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<ApiResponse<IEnumerable<LaboratoryDocumentResponse>>>> GetPendingDocuments()
//        {
//            _logger.LogInformation("Attempting to get all pending laboratory documents by user {UserId}", GetCurrentUserId());
//            try
//            {
//                var documents = await _documentService.GetPendingDocumentsAsync();
//                return Ok(ApiResponse<IEnumerable<LaboratoryDocumentResponse>>.Success(documents, "Pending documents retrieved successfully"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting pending laboratory documents");
//                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while getting pending documents", new[] { ex.Message }, 500));
//            }
//        }
//        #endregion
//    }
//}
