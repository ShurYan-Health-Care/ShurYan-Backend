using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/laboratories/{laboratoryId}/[controller]")]
    //[Authorize]
    public class LaboratoryDocumentsController : ControllerBase
    {
        private readonly ILaboratoryDocumentService _documentService;
        private readonly ILogger<LaboratoryDocumentsController> _logger;

        public LaboratoryDocumentsController(
            ILaboratoryDocumentService documentService,
            ILogger<LaboratoryDocumentsController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all documents for a laboratory
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LaboratoryDocumentResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LaboratoryDocumentResponse>>> GetLaboratoryDocuments(Guid laboratoryId)
        {
            try
            {
                var documents = await _documentService.GetLaboratoryDocumentsAsync(laboratoryId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents for laboratory {LaboratoryId}", laboratoryId);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get document by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LaboratoryDocumentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LaboratoryDocumentResponse>> GetDocument(Guid laboratoryId, Guid id)
        {
            try
            {
                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document == null)
                    return NotFound(new { Message = $"Document with ID {id} not found" });

                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document {DocumentId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Upload a new document
        /// </summary>
        [HttpPost]
        //[Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(typeof(LaboratoryDocumentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LaboratoryDocumentResponse>> UploadDocument(
            Guid laboratoryId,
            [FromBody] CreateLaboratoryDocumentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var document = await _documentService.UploadDocumentAsync(laboratoryId, request);
                return CreatedAtAction(
                    nameof(GetDocument),
                    new { laboratoryId, id = document.Id },
                    document);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for laboratory {LaboratoryId}", laboratoryId);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Delete document
        /// </summary>
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Laboratory,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteDocument(Guid laboratoryId, Guid id)
        {
            try
            {
                var result = await _documentService.DeleteDocumentAsync(id);
                if (!result)
                    return NotFound(new { Message = $"Document with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Approve document
        /// </summary>
        [HttpPost("{id}/approve")]
        //[Authorize(Roles = "Admin,Verifier")]
        [ProducesResponseType(typeof(LaboratoryDocumentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LaboratoryDocumentResponse>> ApproveDocument(Guid laboratoryId, Guid id)
        {
            try
            {
                var document = await _documentService.ApproveDocumentAsync(id);
                return Ok(document);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving document {DocumentId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Reject document
        /// </summary>
        [HttpPost("{id}/reject")]
        //[Authorize(Roles = "Admin,Verifier")]
        [ProducesResponseType(typeof(LaboratoryDocumentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LaboratoryDocumentResponse>> RejectDocument(
            Guid laboratoryId,
            Guid id,
            [FromBody] RejectDocumentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var document = await _documentService.RejectDocumentAsync(id, request.RejectionReason);
                return Ok(document);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting document {DocumentId}", id);
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get pending documents for verification
        /// </summary>
        [HttpGet("~/api/laboratory-documents/pending")]
        //[Authorize(Roles = "Admin,Verifier")]
        [ProducesResponseType(typeof(IEnumerable<LaboratoryDocumentResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LaboratoryDocumentResponse>>> GetPendingDocuments()
        {
            try
            {
                var documents = await _documentService.GetPendingDocumentsAsync();
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending documents");
                return StatusCode(500, new { Message = "An error occurred" });
            }
        }
    }

    // Helper request model
    public class RejectDocumentRequest
    {
        public string RejectionReason { get; set; } = string.Empty;
    }
}
