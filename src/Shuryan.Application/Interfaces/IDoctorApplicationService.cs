using Shuryan.Application.DTOs.Requests.Doctor;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Shuryan.Application.Interfaces
{
    public interface IDoctorApplicationService
    {
        // ==================== DOCTOR CRUD ====================

        Task<DoctorResponse?> GetDoctorByIdAsync(Guid id);
        Task<DoctorResponse?> GetDoctorByEmailAsync(string email);
        Task<IEnumerable<DoctorResponse>> GetDoctorsBySpecialtyAsync(MedicalSpecialty specialty);
        Task<IEnumerable<DoctorResponse>> GetVerifiedDoctorsAsync();
        Task<IEnumerable<DoctorResponse>> GetDoctorsByGovernorateAsync(Governorate governorate);
        Task<PaginatedResponse<DoctorResponse>> SearchDoctorsAsync(SearchDoctorsRequest request);
        Task<bool> IsDoctorAvailableAtAsync(Guid doctorId, DateTime dateTime);
        Task<DoctorResponse> CreateDoctorAsync(CreateDoctorRequest request);
        Task<DoctorResponse> UpdateDoctorAsync(Guid id, UpdateDoctorRequest request);
        Task<bool> DeleteDoctorAsync(Guid id);
        Task<bool> VerifyDoctorAsync(Guid id, VerifyDoctorRequest request);
        
        // ==================== AVAILABILITY ====================
        Task<IEnumerable<DoctorAvailabilityResponse>> GetDoctorAvailabilitiesAsync(Guid doctorId);
        Task<DoctorAvailabilityResponse> AddDoctorAvailabilityAsync(
            Guid doctorId,
            CreateDoctorAvailabilityRequest request);
        Task<DoctorAvailabilityResponse> UpdateDoctorAvailabilityAsync(
            Guid doctorId,
            Guid availabilityId,
            UpdateDoctorAvailabilityRequest request);
        Task<bool> DeleteDoctorAvailabilityAsync(Guid doctorId, Guid availabilityId);

        // ==================== CONSULTATIONS ====================
        Task<IEnumerable<DoctorConsultationResponse>> GetDoctorConsultationsAsync(Guid doctorId);
        Task<DoctorConsultationResponse> AddDoctorConsultationAsync(
            Guid doctorId,
            CreateDoctorConsultationRequest request);
        Task<DoctorConsultationResponse> UpdateDoctorConsultationAsync(
            Guid doctorId,
            Guid consultationId,
            UpdateDoctorConsultationRequest request);
        Task<bool> DeleteDoctorConsultationAsync(Guid doctorId, Guid consultationId);

        // ==================== DOCUMENTS ====================
        Task<IEnumerable<DoctorDocumentResponse>> GetDoctorDocumentsAsync(Guid doctorId);
        Task<DoctorDocumentResponse> AddDoctorDocumentAsync(
            Guid doctorId,
            CreateDoctorDocumentRequest request);
        Task<bool> DeleteDoctorDocumentAsync(Guid doctorId, Guid documentId);

        // ==================== OVERRIDES ====================
        
        Task<IEnumerable<DoctorOverrideResponse>> GetDoctorOverridesAsync(Guid doctorId);
        Task<DoctorOverrideResponse> AddDoctorOverrideAsync(
            Guid doctorId,
            CreateDoctorOverrideRequest request);
        Task<bool> DeleteDoctorOverrideAsync(Guid doctorId, Guid overrideId);
    }
}