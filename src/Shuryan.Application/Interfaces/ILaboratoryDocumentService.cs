using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Core.Enums; // Assuming this namespace exists for verification enums if needed later
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    public interface ILaboratoryDocumentService
    {
        #region CRUD Operations

        /// <summary>
        /// Get all documents for a laboratory
        /// </summary>
        Task<IEnumerable<LaboratoryDocumentResponse>> GetLaboratoryDocumentsAsync(Guid laboratoryId);

        /// <summary>
        /// Get document by ID
        /// </summary>
        Task<LaboratoryDocumentResponse?> GetDocumentByIdAsync(Guid id);

        /// <summary>
        /// Upload a new document
        /// </summary>
        Task<LaboratoryDocumentResponse> UploadDocumentAsync(Guid laboratoryId, CreateLaboratoryDocumentRequest request);

        /// <summary>
        /// Delete document
        /// </summary>
        Task<bool> DeleteDocumentAsync(Guid id);

        #endregion

        #region Verification Operations

        /// <summary>
        /// Approve document
        /// </summary>
        Task<LaboratoryDocumentResponse> ApproveDocumentAsync(Guid id);

        /// <summary>
        /// Reject document
        /// </summary>
        Task<LaboratoryDocumentResponse> RejectDocumentAsync(Guid id, string rejectionReason);

        /// <summary>
        /// Get pending documents for verification
        /// </summary>
        Task<IEnumerable<LaboratoryDocumentResponse>> GetPendingDocumentsAsync();

        #endregion
    }
}
