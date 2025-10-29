using Shuryan.Application.DTOs.Requests.Clinic;
using Shuryan.Application.DTOs.Responses.Clinic;
using Shuryan.Application.DTOs.Common.Pagination;
using System;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Service مسؤول عن إدارة الشركاء المقترحين (صيدليات - معامل)
    /// </summary>
    public interface IDoctorPartnerService
    {
        #region Current Suggested Partner
        /// <summary>
        /// جلب الشركاء المقترحين حالياً (صيدلية و/أو معمل)
        /// </summary>
        Task<SuggestedPartnerResponse> GetSuggestedPartnerAsync(Guid doctorId);

        /// <summary>
        /// اقتراح شريك جديد (صيدلية و/أو معمل)
        /// يمكن إضافة صيدلية واحدة ومعمل واحد في نفس الوقت
        /// </summary>
        Task<SuggestedPartnerResponse> SuggestPartnerAsync(Guid doctorId, SuggestPartnerRequest request);

        /// <summary>
        /// إزالة الشركاء المقترحين
        /// </summary>
        Task<bool> RemoveSuggestedPartnerAsync(Guid doctorId);
        #endregion

        #region Available Partners
        /// <summary>
        /// جلب قائمة الصيدليات المتاحة مع Pagination
        /// </summary>
        Task<PaginatedResponse<PartnerResponse>> GetAvailablePharmaciesAsync(PaginationParams paginationParams);

        /// <summary>
        /// جلب قائمة المعامل المتاحة مع Pagination
        /// </summary>
        Task<PaginatedResponse<PartnerResponse>> GetAvailableLaboratoriesAsync(PaginationParams paginationParams);
        #endregion
    }
}
