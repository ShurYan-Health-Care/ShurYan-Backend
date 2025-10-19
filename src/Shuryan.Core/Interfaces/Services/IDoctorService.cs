using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical.Schedules;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Identity;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Shuryan.Core.Enums.Appointments;

namespace Shuryan.Core.Interfaces.Services
{
    public interface IDoctorService
    {
        // ==================== DOCTOR CRUD ====================
        
        Task<Doctor?> GetDoctorByIdAsync(Guid id);
        Task<Doctor?> GetDoctorByEmailAsync(string email);
        Task<IEnumerable<Doctor>> GetDoctorsBySpecialtyAsync(MedicalSpecialty specialty);
        Task<IEnumerable<Doctor>> GetVerifiedDoctorsAsync();
        Task<IEnumerable<Doctor>> GetDoctorsByGovernorateAsync(Governorate governorate);
        
        Task<IEnumerable<Doctor>> SearchDoctorsAsync(
            string? searchTerm = null,
            MedicalSpecialty? specialty = null,
            Governorate? governorate = null,
            int? minYearsOfExperience = null,
            decimal? maxConsultationFee = null,
            double? minRating = null
        );
        
        Task<bool> IsDoctorAvailableAtAsync(Guid doctorId, DateTime dateTime);
        Task<Doctor> CreateDoctorAsync(Doctor doctor);
        Task<Doctor> UpdateDoctorAsync(Guid id, Doctor doctor);
        Task<bool> DeleteDoctorAsync(Guid id);
        Task<bool> VerifyDoctorAsync(Guid id, VerificationStatus status, Guid? verifierId);

        // ==================== AVAILABILITY ====================
     
        Task<IEnumerable<DoctorAvailability>> GetDoctorAvailabilitiesAsync(Guid doctorId);
        
        Task<DoctorAvailability> AddDoctorAvailabilityAsync(DoctorAvailability availability);
        
        Task<DoctorAvailability> UpdateDoctorAvailabilityAsync(
            Guid doctorId, 
            Guid availabilityId, 
            SysDayOfWeek? dayOfWeek = null,
            TimeOnly? startTime = null,
            TimeOnly? endTime = null);
        
        Task<bool> DeleteDoctorAvailabilityAsync(Guid doctorId, Guid availabilityId);

        // ==================== CONSULTATIONS ====================
        
        Task<IEnumerable<DoctorConsultation>> GetDoctorConsultationsAsync(Guid doctorId);
        
        Task<DoctorConsultation> AddDoctorConsultationAsync(
            DoctorConsultation consultation, 
            ConsultationTypeEnum consultationType);
        
        Task<DoctorConsultation> UpdateDoctorConsultationAsync(
            Guid doctorId,
            Guid consultationId,
            decimal? consultationFee = null,
            int? sessionDurationMinutes = null);
       
        Task<bool> DeleteDoctorConsultationAsync(Guid doctorId, Guid consultationId);

        // ==================== DOCUMENTS ====================
        
        Task<IEnumerable<DoctorDocument>> GetDoctorDocumentsAsync(Guid doctorId);
        Task<DoctorDocument> AddDoctorDocumentAsync(DoctorDocument document);
        Task<bool> DeleteDoctorDocumentAsync(Guid doctorId, Guid documentId);

        // ==================== OVERRIDES ====================
        
        Task<IEnumerable<DoctorOverride>> GetDoctorOverridesAsync(Guid doctorId);
        Task<DoctorOverride> AddDoctorOverrideAsync(DoctorOverride overrideSchedule);
        Task<bool> DeleteDoctorOverrideAsync(Guid doctorId, Guid overrideId);
    }
}