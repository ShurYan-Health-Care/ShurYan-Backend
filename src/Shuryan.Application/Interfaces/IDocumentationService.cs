using System;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Requests.Documentation;
using Shuryan.Application.DTOs.Responses.Documentation;

namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Service مسؤول عن إدارة توثيق الكشف (Consultation Documentation)
    /// </summary>
    public interface IDocumentationService
    {
        /// <summary>
        /// حفظ أو تحديث توثيق الكشف (Doctor only)
        /// </summary>
        Task<DocumentationResponse> SaveDocumentationAsync(Guid appointmentId, Guid doctorId, SaveDocumentationRequest request);

        /// <summary>
        /// الحصول على توثيق الكشف (Doctor and Patient)
        /// </summary>
        Task<DocumentationResponse?> GetDocumentationAsync(Guid appointmentId, Guid userId, bool isDoctor);
    }
}
