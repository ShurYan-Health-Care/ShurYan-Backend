using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.DTOs.Responses.Pharmacy;
using Shuryan.Application.DTOs.Responses.Laboratory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Service interface لعمليات الـ Verifier - إدارة حالات التحقق للأطباء والصيدليات والمعامل
    /// </summary>
    public interface IVerifierService
    {
        #region Doctor Verification Status Management

        Task<bool> StartDoctorReviewAsync(Guid doctorId);
        Task<bool> VerifyDoctorAsync(Guid doctorId, Guid verifierId);
        Task<bool> RejectDoctorAsync(Guid doctorId);

        #endregion

        #region Get Doctors by Verification Status

        Task<PaginatedResponse<DoctorVerificationListResponse>> GetDoctorsWithSentStatusAsync(PaginationParams paginationParams);
        Task<PaginatedResponse<DoctorVerificationListResponse>> GetDoctorsUnderReviewAsync(PaginationParams paginationParams);
        Task<PaginatedResponse<DoctorVerificationListResponse>> GetVerifiedDoctorsAsync(PaginationParams paginationParams, Guid verifierId);
        Task<PaginatedResponse<DoctorVerificationListResponse>> GetRejectedDoctorsAsync(PaginationParams paginationParams);

        #endregion

        #region Document Verification

        Task<List<DoctorDocumentItemResponse>> GetDoctorDocumentsAsync(Guid doctorId);
        Task<bool> ApproveDocumentAsync(Guid documentId);
        Task<bool> RejectDocumentAsync(Guid documentId, string? rejectionReason);

        // Pharmacy document approval/rejection
        Task<bool> ApprovePharmacyDocumentAsync(Guid documentId);
        Task<bool> RejectPharmacyDocumentAsync(Guid documentId, string? rejectionReason);

        // Laboratory document approval/rejection
        Task<bool> ApproveLaboratoryDocumentAsync(Guid documentId);
        Task<bool> RejectLaboratoryDocumentAsync(Guid documentId, string? rejectionReason);

        #endregion

        #region Pharmacy Verification Status Management

        Task<bool> StartPharmacyReviewAsync(Guid pharmacyId);
        Task<bool> VerifyPharmacyAsync(Guid pharmacyId, Guid verifierId);
        Task<bool> RejectPharmacyAsync(Guid pharmacyId);

        #endregion

        #region Get Pharmacies by Verification Status

        Task<PaginatedResponse<PharmacyVerificationListResponse>> GetPharmaciesWithSentStatusAsync(PaginationParams paginationParams);
        Task<PaginatedResponse<PharmacyVerificationListResponse>> GetPharmaciesUnderReviewAsync(PaginationParams paginationParams);
        Task<PaginatedResponse<PharmacyVerificationListResponse>> GetVerifiedPharmaciesAsync(PaginationParams paginationParams, Guid verifierId);
        Task<PaginatedResponse<PharmacyVerificationListResponse>> GetRejectedPharmaciesAsync(PaginationParams paginationParams);
        Task<List<PharmacyDocumentItemResponse>> GetPharmacyDocumentsAsync(Guid pharmacyId);

        #endregion

        #region Laboratory Verification Status Management

        Task<bool> StartLaboratoryReviewAsync(Guid laboratoryId);
        Task<bool> VerifyLaboratoryAsync(Guid laboratoryId, Guid verifierId);
        Task<bool> RejectLaboratoryAsync(Guid laboratoryId);

        #endregion

        #region Get Laboratories by Verification Status

        Task<PaginatedResponse<LaboratoryVerificationListResponse>> GetLaboratoriesWithSentStatusAsync(PaginationParams paginationParams);
        Task<PaginatedResponse<LaboratoryVerificationListResponse>> GetLaboratoriesUnderReviewAsync(PaginationParams paginationParams);
        Task<PaginatedResponse<LaboratoryVerificationListResponse>> GetVerifiedLaboratoriesAsync(PaginationParams paginationParams, Guid verifierId);
        Task<PaginatedResponse<LaboratoryVerificationListResponse>> GetRejectedLaboratoriesAsync(PaginationParams paginationParams);
        Task<List<LaboratoryDocumentItemResponse>> GetLaboratoryDocumentsAsync(Guid laboratoryId);

        #endregion
    }
}

