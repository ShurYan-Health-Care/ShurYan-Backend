using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests;
using Shuryan.Application.DTOs.Responses.Doctor;
using System;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Service interface لعمليات الـ Verifier - إدارة حالات التحقق للأطباء
    /// </summary>
    public interface IVerifierService
    {
        #region Doctor Verification Status Management
        
        /// <summary>
        /// بدء مراجعة الدكتور - تغيير حالة التحقق إلى "تحت المراجعة"
        /// </summary>
        Task<bool> StartDoctorReviewAsync(Guid doctorId);

        /// <summary>
        /// اعتماد الدكتور - تغيير حالة التحقق إلى "معتمد"
        /// </summary>
        Task<bool> VerifyDoctorAsync(Guid doctorId, Guid verifierId);

        /// <summary>
        /// رفض طلب الدكتور - تغيير حالة التحقق إلى "مرفوض"
        /// </summary>
        Task<bool> RejectDoctorAsync(Guid doctorId);

        #endregion

        #region Get Doctors by Verification Status

        /// <summary>
        /// جلب جميع الأطباء الذين حالتهم "مُرسل"
        /// </summary>
        Task<PaginatedResponse<DoctorVerificationListResponse>> GetDoctorsWithSentStatusAsync(PaginationParams paginationParams);

        /// <summary>
        /// جلب جميع الأطباء الذين حالتهم "تحت المراجعة"
        /// </summary>
        Task<PaginatedResponse<DoctorVerificationListResponse>> GetDoctorsUnderReviewAsync(PaginationParams paginationParams);

        /// <summary>
        /// جلب جميع الأطباء المعتمدين من قبل الـ Verifier المحدد
        /// </summary>
        Task<PaginatedResponse<DoctorVerificationListResponse>> GetVerifiedDoctorsAsync(PaginationParams paginationParams, Guid verifierId);

        /// <summary>
        /// جلب جميع الأطباء الذين حالتهم "مرفوض"
        /// </summary>
        Task<PaginatedResponse<DoctorVerificationListResponse>> GetRejectedDoctorsAsync(PaginationParams paginationParams);

        #endregion

        #region Document Verification

        /// <summary>
        /// جلب مستندات دكتور معين
        /// </summary>
        Task<List<DoctorDocumentItemResponse>> GetDoctorDocumentsAsync(Guid doctorId);

        /// <summary>
        /// قبول مستند الدكتور
        /// </summary>
        Task<bool> ApproveDocumentAsync(Guid documentId);

        /// <summary>
        /// رفض مستند الدكتور مع سبب الرفض
        /// </summary>
        Task<bool> RejectDocumentAsync(Guid documentId, string? rejectionReason);

        #endregion
    }
}
