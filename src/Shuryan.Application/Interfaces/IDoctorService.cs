using Shuryan.Application.DTOs.Common.Pagination;
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

        #region Dashboard Operations
        Task<DoctorDashboardStatsResponse> GetDashboardStatsAsync(Guid doctorId);
        Task<PaginatedResponse<TodayAppointmentResponse>> GetTodayAppointmentsAsync(Guid doctorId, PaginationParams paginationParams);
        #endregion

        #region Public Doctor Directory Operations
        Task<PaginatedResponse<DoctorListItemResponse>> GetDoctorsListAsync(SearchDoctorsRequest searchRequest);

        Task<DoctorDetailsWithClinicResponse?> GetDoctorDetailsWithClinicAsync(Guid doctorId);
        #endregion

        #region Doctor Patient Management Operations
        Task<PaginatedResponse<DoctorPatientResponse>> GetDoctorPatientsWithPaginationAsync(Guid doctorId, PaginationParams paginationParams);
        
        Task<IEnumerable<DoctorPatientListItemResponse>> GetDoctorPatientsAsync(Guid doctorId);

        Task<PatientMedicalRecordResponse?> GetPatientMedicalRecordAsync(Guid patientId, Guid doctorId);

        Task<PatientSessionDocumentationListResponse?> GetPatientSessionDocumentationsAsync(Guid patientId, Guid doctorId);

        Task<PatientPrescriptionsListResponse?> GetPatientPrescriptionsAsync(Guid patientId, Guid doctorId);
        #endregion

        #region Verification Operations
        Task<bool> SubmitForReviewAsync(Guid doctorId);
        #endregion
    }
}
