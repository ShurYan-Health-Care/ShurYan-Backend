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
        /// <summary>
        /// الحصول على قائمة الدكاترة مع pagination - معلومات مختصرة للعرض في القائمة
        /// </summary>
        Task<PaginatedResponse<DoctorListItemResponse>> GetDoctorsListAsync(PaginationParams paginationParams);

        /// <summary>
        /// الحصول على التفاصيل الكاملة للدكتور مع معلومات العيادة
        /// </summary>
        Task<DoctorDetailsWithClinicResponse?> GetDoctorDetailsWithClinicAsync(Guid doctorId);
        #endregion

        #region Doctor Patient Management Operations
        /// <summary>
        /// الحصول على قائمة المرضى الذين تعاملوا مع الدكتور (مع pagination)
        /// </summary>
        Task<PaginatedResponse<DoctorPatientResponse>> GetDoctorPatientsWithPaginationAsync(Guid doctorId, PaginationParams paginationParams);
        
        /// <summary>
        /// الحصول على قائمة المرضى الذين تعاملوا مع الدكتور
        /// </summary>
        Task<IEnumerable<DoctorPatientListItemResponse>> GetDoctorPatientsAsync(Guid doctorId);

        /// <summary>
        /// الحصول على السجل الطبي الكامل لمريض معين
        /// </summary>
        Task<PatientMedicalRecordResponse?> GetPatientMedicalRecordAsync(Guid patientId, Guid doctorId);

        /// <summary>
        /// الحصول على توثيق جميع الجلسات لمريض معين مع الدكتور
        /// </summary>
        Task<PatientSessionDocumentationListResponse?> GetPatientSessionDocumentationsAsync(Guid patientId, Guid doctorId);

        /// <summary>
        /// الحصول على جميع الروشتات لمريض معين من الدكتور
        /// </summary>
        Task<PatientPrescriptionsListResponse?> GetPatientPrescriptionsAsync(Guid patientId, Guid doctorId);
        #endregion

        #region Verification Operations
        /// <summary>
        /// تقديم طلب المراجعة - تغيير حالة التحقق إلى "مُرسل"
        /// </summary>
        Task<bool> SubmitForReviewAsync(Guid doctorId);
        #endregion
    }
}
