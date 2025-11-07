using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Doctor;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/doctors/me/documents")]
    [Authorize(Roles = "Doctor")]
    public class DoctorDocumentsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly ILogger<DoctorDocumentsController> _logger;

        public DoctorDocumentsController(
            IDoctorService doctorService,
            ILogger<DoctorDocumentsController> logger)
        {
            _doctorService = doctorService;
            _logger = logger;
        }

        #region GET Operations
        [HttpGet("{documentId}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DoctorDocumentResponse>>> GetDocumentById(Guid documentId)
        {
            _logger.LogInformation("Getting document by ID: {DocumentId}", documentId);
            
            try
            {
                var document = await _doctorService.GetDocumentByIdAsync(documentId);
                if (document == null)
                {
                    _logger.LogWarning("Document not found: {DocumentId}", documentId);
                    return NotFound(ApiResponse<object>.Failure(
                        $"Document with ID {documentId} not found",
                        statusCode: 404
                    ));
                }

                return Ok(ApiResponse<DoctorDocumentResponse>.Success(
                    document,
                    "Document retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document by ID: {DocumentId}", documentId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An error occurred while getting the document",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("required")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorDocumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DoctorDocumentResponse>>>> GetMyRequiredDocuments()
        {
            var currentDoctorId = GetCurrentDoctorId();
            _logger.LogInformation("Get required documents request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var documents = await _doctorService.GetRequiredDocumentsAsync(currentDoctorId);
                _logger.LogInformation("Retrieved {Count} required documents for doctor: {DoctorId}",
                    documents.Count(), currentDoctorId);

                return Ok(ApiResponse<IEnumerable<DoctorDocumentResponse>>.Success(
                    documents,
                    "Required documents retrieved successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving required documents for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving required documents",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("research")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorDocumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DoctorDocumentResponse>>>> GetMyResearchPapers()
        {
            var currentDoctorId = GetCurrentDoctorId();
            _logger.LogInformation("Get research papers request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var documents = await _doctorService.GetResearchPapersAsync(currentDoctorId);
                _logger.LogInformation("Retrieved {Count} research papers for doctor: {DoctorId}",
                    documents.Count(), currentDoctorId);

                return Ok(ApiResponse<IEnumerable<DoctorDocumentResponse>>.Success(
                    documents,
                    "Research papers retrieved successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving research papers for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving research papers",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("awards")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorDocumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DoctorDocumentResponse>>>> GetMyAwardCertificates()
        {
            var currentDoctorId = GetCurrentDoctorId();
            _logger.LogInformation("Get awards/certificates request for doctor: {DoctorId}", currentDoctorId);

            try
            {
                var documents = await _doctorService.GetAwardCertificatesAsync(currentDoctorId);
                _logger.LogInformation("Retrieved {Count} awards/certificates for doctor: {DoctorId}",
                    documents.Count(), currentDoctorId);

                return Ok(ApiResponse<IEnumerable<DoctorDocumentResponse>>.Success(
                    documents,
                    "Awards and certificates retrieved successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving awards/certificates for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving awards and certificates",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region POST Operations

        [HttpPost("required")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<DoctorDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<DoctorDocumentResponse>>> UploadOrUpdateRequiredDocument(
            [FromForm] UploadDoctorDocumentRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();
            _logger.LogInformation("Upload/Update required document request for doctor: {DoctorId}, Type: {Type}",
                currentDoctorId, request.Type);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for required document upload. Errors: {Errors}",
                    string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
            }

            try
            {
                var document = await _doctorService.UploadOrUpdateRequiredDocumentAsync(currentDoctorId, request);
                _logger.LogInformation("Required document {DocumentId} uploaded/updated successfully for doctor {DoctorId}",
                    document.Id, currentDoctorId);

                return Ok(ApiResponse<DoctorDocumentResponse>.Success(
                    document,
                    "Required document uploaded/updated successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request on required document upload for doctor {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on required document upload for doctor {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading/updating required document for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while uploading/updating the required document",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("awards")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<DoctorDocumentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<DoctorDocumentResponse>>> UploadAwardCertificate(
            [FromForm] UploadDoctorDocumentRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();
            _logger.LogInformation("Upload award/certificate request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for award upload. Errors: {Errors}",
                    string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
            }

            try
            {
                var document = await _doctorService.UploadOrUpdateAwardCertificateAsync(currentDoctorId, request);
                _logger.LogInformation("Award/certificate {DocumentId} uploaded successfully for doctor {DoctorId}",
                    document.Id, currentDoctorId);

                var response = ApiResponse<DoctorDocumentResponse>.Success(
                    document,
                    "Award/certificate uploaded successfully",
                    201
                );
                
                return CreatedAtAction(
                    nameof(GetDocumentById),
                    new { documentId = document.Id },
                    response
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request on award upload for doctor {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on award upload for doctor {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading award/certificate for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while uploading the award/certificate",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("research")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<DoctorDocumentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<DoctorDocumentResponse>>> UploadResearchPaper(
            [FromForm] UploadDoctorDocumentRequest request)
        {
            var currentDoctorId = GetCurrentDoctorId();
            _logger.LogInformation("Upload research paper request for doctor: {DoctorId}", currentDoctorId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Invalid model state for research paper upload. Errors: {Errors}",
                    string.Join(", ", errors));
                return BadRequest(ApiResponse<object>.Failure("Invalid request data", errors, 400));
            }

            try
            {
                var document = await _doctorService.UploadOrUpdateResearchPaperAsync(currentDoctorId, request);
                _logger.LogInformation("Research paper {DocumentId} uploaded successfully for doctor {DoctorId}",
                    document.Id, currentDoctorId);

                var response = ApiResponse<DoctorDocumentResponse>.Success(
                    document,
                    "Research paper uploaded successfully",
                    201
                );
                
                return CreatedAtAction(
                    nameof(GetDocumentById),
                    new { documentId = document.Id },
                    response
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request on research paper upload for doctor {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on research paper upload for doctor {DoctorId}", currentDoctorId);
                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading research paper for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while uploading the research paper",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Helper Methods

        private Guid GetCurrentDoctorId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        #endregion
    }
}
