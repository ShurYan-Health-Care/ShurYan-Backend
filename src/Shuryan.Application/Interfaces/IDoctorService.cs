using Shuryan.Application.DTOs.Requests.Doctor;
using Shuryan.Application.DTOs.Responses.Doctor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    public interface IDoctorService
    {
        #region Profile Operations - GET
        Task<DoctorProfileResponse?> GetDoctorProfileAsync(Guid doctorId);
        Task<DoctorPersonalProfileResponse?> GetPersonalProfileAsync(Guid doctorId);
        Task<DoctorProfessionalInfoResponse?> GetProfessionalInfoAsync(Guid doctorId);
        Task<DoctorSpecialtyExperienceResponse?> GetSpecialtyExperienceAsync(Guid doctorId);
        #endregion

        #region Profile Operations - UPDATE
        Task<DoctorProfileResponse> UpdateDoctorProfileAsync(Guid doctorId, UpdateDoctorProfileRequest request);
        Task<DoctorProfileResponse> UpdatePersonalInfoAsync(Guid doctorId, UpdatePersonalInfoRequest request);
        Task<DoctorSpecialtyExperienceResponse> UpdateSpecialtyExperienceAsync(Guid doctorId, UpdateSpecialtyExperienceRequest request);
        #endregion

        #region Document Operations - GET
        Task<DoctorDocumentResponse?> GetDocumentByIdAsync(Guid documentId);
        Task<IEnumerable<DoctorDocumentResponse>> GetRequiredDocumentsAsync(Guid doctorId);
        Task<IEnumerable<DoctorDocumentResponse>> GetResearchPapersAsync(Guid doctorId);
        Task<IEnumerable<DoctorDocumentResponse>> GetAwardCertificatesAsync(Guid doctorId);
        #endregion

        #region Document Operations - UPLOAD
        Task<DoctorDocumentResponse> UploadOrUpdateRequiredDocumentAsync(Guid doctorId, UploadDoctorDocumentRequest request);
        Task<DoctorDocumentResponse> UploadOrUpdateAwardCertificateAsync(Guid doctorId, UploadDoctorDocumentRequest request);
        Task<DoctorDocumentResponse> UploadOrUpdateResearchPaperAsync(Guid doctorId, UploadDoctorDocumentRequest request);
        #endregion

        #region Utilities
        IEnumerable<SpecialtyResponse> GetSpecialties();
        #endregion
    }
}
