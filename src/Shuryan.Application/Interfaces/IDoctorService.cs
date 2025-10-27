using Shuryan.Application.DTOs.Requests.Doctor;
using Shuryan.Application.DTOs.Responses.Doctor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    public interface IDoctorService
    {
        #region Profile Management
        Task<DoctorProfileResponse?> GetDoctorProfileAsync(Guid doctorId);
        Task<DoctorPersonalProfileResponse?> GetPersonalProfileAsync(Guid doctorId);
        Task<DoctorProfileResponse> UpdateDoctorProfileAsync(Guid doctorId, UpdateDoctorProfileRequest request);
        Task<DoctorProfileResponse> UpdatePersonalInfoAsync(Guid doctorId, UpdatePersonalInfoRequest request);
        Task<IEnumerable<DoctorProfileResponse>> GetAllDoctorsAsync(int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<DoctorProfileResponse>> SearchDoctorsAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<DoctorProfileResponse>> GetDoctorsBySpecialtyAsync(string specialty, int pageNumber = 1, int pageSize = 10);
        IEnumerable<SpecialtyResponse> GetSpecialties();
        Task<bool> DeleteDoctorAsync(Guid doctorId);
        Task<DoctorProfileResponse> UpdateProfileImageAsync(Guid doctorId, string imageUrl);
        Task<IEnumerable<DoctorProfileResponse>> GetTopRatedDoctorsAsync(int count = 10);
        Task<IEnumerable<DoctorProfileResponse>> GetDoctorsByGovernorateAsync(string governorate, int pageNumber = 1, int pageSize = 10);
        Task<DoctorStatisticsResponse> GetDoctorStatisticsAsync(Guid doctorId);
        #endregion

        #region Document Management
        Task<IEnumerable<DoctorDocumentResponse>> GetDoctorDocumentsAsync(Guid doctorId);
        Task<DoctorDocumentResponse?> GetDocumentByIdAsync(Guid documentId);
        Task<DoctorDocumentResponse> UploadDocumentAsync(Guid doctorId, UploadDoctorDocumentRequest request);
        Task<DoctorDocumentResponse> UpdateDocumentAsync(Guid documentId, UploadDoctorDocumentRequest request);
        Task<bool> DeleteDocumentAsync(Guid documentId);
        Task<DoctorDocumentResponse> SubmitDocumentForReviewAsync(Guid documentId); // Draft → Pending
        Task<DoctorDocumentResponse> ApproveDocumentAsync(Guid documentId); // Pending → Approved
        Task<DoctorDocumentResponse> RejectDocumentAsync(Guid documentId, string rejectionReason); // Pending → Rejected
        Task<DoctorDocumentResponse> MarkDocumentAsExpiredAsync(Guid documentId); // Approved → Expired
        Task<IEnumerable<DoctorDocumentResponse>> GetPendingDocumentsAsync();
        Task<IEnumerable<DoctorDocumentResponse>> GetApprovedDocumentsAsync();
        Task<IEnumerable<DoctorDocumentResponse>> GetRejectedDocumentsAsync();
        #endregion

        #region Verification
        Task<DoctorProfileResponse> VerifyDoctorAsync(Guid doctorId, Guid verifierId);
        Task<DoctorProfileResponse> UnverifyDoctorAsync(Guid doctorId);
        Task<IEnumerable<DoctorProfileResponse>> GetPendingVerificationDoctorsAsync(int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<DoctorProfileResponse>> GetVerifiedDoctorsAsync(int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<DoctorProfileResponse>> GetUnverifiedDoctorsAsync(int pageNumber = 1, int pageSize = 10);
        Task<DoctorProfileResponse> SuspendDoctorAsync(Guid doctorId);
        Task<DoctorProfileResponse> ActivateDoctorAsync(Guid doctorId); 
        #endregion
    }
}
