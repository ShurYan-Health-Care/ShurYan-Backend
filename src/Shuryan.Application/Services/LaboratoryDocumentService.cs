using AutoMapper;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Core.Interfaces.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{
    public class LaboratoryDocumentService : ILaboratoryDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LaboratoryDocumentService> _logger;

        public LaboratoryDocumentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<LaboratoryDocumentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<LaboratoryDocumentResponse>> GetLaboratoryDocumentsAsync(Guid laboratoryId)
        {
            try
            {
                var documents = await _unitOfWork.LaboratoryDocuments.GetAllAsync();
                var labDocuments = documents.Where(d => d.LaboratoryId == laboratoryId).ToList();

                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(laboratoryId);
                var laboratoryName = laboratory?.Name ?? "";

                var responses = labDocuments.Select(doc => new LaboratoryDocumentResponse
                {
                    Id = doc.Id,
                    DocumentUrl = doc.DocumentUrl,
                    Type = doc.Type,
                    TypeName = doc.Type.ToString(),
                    Status = doc.Status,
                    StatusName = doc.Status.ToString(),
                    RejectionReason = doc.RejectionReason,
                    LaboratoryId = doc.LaboratoryId,
                    LaboratoryName = laboratoryName,
                    CreatedAt = doc.CreatedAt,
                    CreatedBy = doc.CreatedBy,
                    UpdatedAt = doc.UpdatedAt,
                    UpdatedBy = doc.UpdatedBy
                }).ToList();

                _logger.LogInformation("Retrieved {Count} documents for laboratory {LaboratoryId}", responses.Count, laboratoryId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents for laboratory {LaboratoryId}", laboratoryId);
                throw;
            }
        }

        public async Task<LaboratoryDocumentResponse?> GetDocumentByIdAsync(Guid id)
        {
            try
            {
                var document = await _unitOfWork.LaboratoryDocuments.GetByIdAsync(id);
                if (document == null)
                    return null;

                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(document.LaboratoryId);
                var laboratoryName = laboratory?.Name ?? "";

                var response = new LaboratoryDocumentResponse
                {
                    Id = document.Id,
                    DocumentUrl = document.DocumentUrl,
                    Type = document.Type,
                    TypeName = document.Type.ToString(),
                    Status = document.Status,
                    StatusName = document.Status.ToString(),
                    RejectionReason = document.RejectionReason,
                    LaboratoryId = document.LaboratoryId,
                    LaboratoryName = laboratoryName,
                    CreatedAt = document.CreatedAt,
                    CreatedBy = document.CreatedBy,
                    UpdatedAt = document.UpdatedAt,
                    UpdatedBy = document.UpdatedBy
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document {DocumentId}", id);
                throw;
            }
        }

        public async Task<LaboratoryDocumentResponse> UploadDocumentAsync(Guid laboratoryId, CreateLaboratoryDocumentRequest request)
        {
            try
            {
                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(laboratoryId);
                if (laboratory == null)
                    throw new ArgumentException($"Laboratory with ID {laboratoryId} not found");

                var document = _mapper.Map<LaboratoryDocument>(request);
                document.Id = Guid.NewGuid();
                document.LaboratoryId = laboratoryId;
                document.Status = VerificationDocumentStatus.Pending;
                document.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.LaboratoryDocuments.AddAsync(document);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Uploaded document {DocumentId} for laboratory {LaboratoryId}", document.Id, laboratoryId);

                return await GetDocumentByIdAsync(document.Id)
                    ?? throw new InvalidOperationException("Failed to retrieve uploaded document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for laboratory {LaboratoryId}", laboratoryId);
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(Guid id)
        {
            try
            {
                var document = await _unitOfWork.LaboratoryDocuments.GetByIdAsync(id);
                if (document == null)
                    return false;

                // Mark as rejected/deleted
                document.Status = VerificationDocumentStatus.Rejected;
                document.RejectionReason = "Document deleted";
                document.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted document {DocumentId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}", id);
                throw;
            }
        }

        public async Task<LaboratoryDocumentResponse> ApproveDocumentAsync(Guid id)
        {
            try
            {
                var document = await _unitOfWork.LaboratoryDocuments.GetByIdAsync(id);
                if (document == null)
                    throw new ArgumentException($"Document with ID {id} not found");

                document.Status = VerificationDocumentStatus.Approved;
                document.RejectionReason = null;
                document.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Approved document {DocumentId}", id);

                return await GetDocumentByIdAsync(id)
                    ?? throw new InvalidOperationException("Failed to retrieve approved document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving document {DocumentId}", id);
                throw;
            }
        }

        public async Task<LaboratoryDocumentResponse> RejectDocumentAsync(Guid id, string rejectionReason)
        {
            try
            {
                var document = await _unitOfWork.LaboratoryDocuments.GetByIdAsync(id);
                if (document == null)
                    throw new ArgumentException($"Document with ID {id} not found");

                document.Status = VerificationDocumentStatus.Rejected;
                document.RejectionReason = rejectionReason;
                document.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Rejected document {DocumentId}", id);

                return await GetDocumentByIdAsync(id)
                    ?? throw new InvalidOperationException("Failed to retrieve rejected document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting document {DocumentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LaboratoryDocumentResponse>> GetPendingDocumentsAsync()
        {
            try
            {
                var documents = await _unitOfWork.LaboratoryDocuments.GetAllAsync();
                var pendingDocuments = documents.Where(d => d.Status == VerificationDocumentStatus.Pending).ToList();

                var responses = new List<LaboratoryDocumentResponse>();
                foreach (var doc in pendingDocuments)
                {
                    var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(doc.LaboratoryId);
                    var laboratoryName = laboratory?.Name ?? "";

                    responses.Add(new LaboratoryDocumentResponse
                    {
                        Id = doc.Id,
                        DocumentUrl = doc.DocumentUrl,
                        Type = doc.Type,
                        TypeName = doc.Type.ToString(),
                        Status = doc.Status,
                        StatusName = doc.Status.ToString(),
                        RejectionReason = doc.RejectionReason,
                        LaboratoryId = doc.LaboratoryId,
                        LaboratoryName = laboratoryName,
                        CreatedAt = doc.CreatedAt,
                        CreatedBy = doc.CreatedBy,
                        UpdatedAt = doc.UpdatedAt,
                        UpdatedBy = doc.UpdatedBy
                    });
                }

                _logger.LogInformation("Retrieved {Count} pending documents", responses.Count);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending documents");
                throw;
            }
        }
    }
}
