using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Entities.Medical.Schedules;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Interfaces.Services;
using Shuryan.Core.Interfaces.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ==================== DOCTOR CRUD ====================

        public async Task<Doctor?> GetDoctorByIdAsync(Guid id)
        {
            return await _unitOfWork.Doctors.GetByIdWithDetailsAsync(id);
        }

        public async Task<Doctor?> GetDoctorByEmailAsync(string email)
        {
            return await _unitOfWork.Doctors.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsBySpecialtyAsync(MedicalSpecialty specialty)
        {
            return await _unitOfWork.Doctors.GetBySpecialtyAsync(specialty);
        }

        public async Task<IEnumerable<Doctor>> GetVerifiedDoctorsAsync()
        {
            return await _unitOfWork.Doctors.GetVerifiedDoctorsAsync();
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsByGovernorateAsync(Governorate governorate)
        {
            return await _unitOfWork.Doctors.GetDoctorsByGovernorateAsync(governorate);
        }

        public async Task<IEnumerable<Doctor>> SearchDoctorsAsync(
            string? searchTerm = null,
            MedicalSpecialty? specialty = null,
            Governorate? governorate = null,
            int? minYearsOfExperience = null,
            decimal? maxConsultationFee = null,
            double? minRating = null)
        {
            return await _unitOfWork.Doctors.SearchDoctorsAsync(
                searchTerm,
                specialty,
                governorate,
                minYearsOfExperience,
                maxConsultationFee,
                minRating
            );
        }

        public async Task<bool> IsDoctorAvailableAtAsync(Guid doctorId, DateTime dateTime)
        {
            return await _unitOfWork.Doctors.IsAvailableAtAsync(doctorId, dateTime);
        }

        public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
        {
            doctor.VerificationStatus = VerificationStatus.Unverified;
            await _unitOfWork.Doctors.AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();
            return doctor;
        }

        public async Task<Doctor> UpdateDoctorAsync(Guid id, Doctor doctor)
        {
            var existingDoctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (existingDoctor == null)
                throw new ArgumentException($"Doctor with ID {id} not found");

            existingDoctor.FirstName = doctor.FirstName;
            existingDoctor.LastName = doctor.LastName;
            existingDoctor.PhoneNumber = doctor.PhoneNumber;
            existingDoctor.MedicalSpecialty = doctor.MedicalSpecialty;
            existingDoctor.YearsOfExperience = doctor.YearsOfExperience;
            existingDoctor.Biography = doctor.Biography;
            existingDoctor.ProfileImageUrl = doctor.ProfileImageUrl;
            existingDoctor.BirthDate = doctor.BirthDate;
            existingDoctor.Gender = doctor.Gender;

            _unitOfWork.Doctors.Update(existingDoctor);
            await _unitOfWork.SaveChangesAsync();
            return existingDoctor;
        }

        public async Task<bool> DeleteDoctorAsync(Guid id)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor == null)
                return false;

            _unitOfWork.Doctors.Delete(doctor);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyDoctorAsync(Guid id, VerificationStatus status, Guid? verifierId)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor == null)
                return false;

            doctor.VerificationStatus = status;
            doctor.VerifiedAt = status == VerificationStatus.Verified ? DateTime.UtcNow : null;
            doctor.VerifierId = verifierId;

            _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // ==================== AVAILABILITY ====================

        public async Task<IEnumerable<DoctorAvailability>> GetDoctorAvailabilitiesAsync(Guid doctorId)
        {
            return await _unitOfWork.DoctorAvailabilities
                .FindAsync(a => a.DoctorId == doctorId && !a.IsDeleted);
        }

        public async Task<DoctorAvailability> AddDoctorAvailabilityAsync(DoctorAvailability availability)
        {
            if (availability == null)
                throw new ArgumentNullException(nameof(availability));

            // التحقق من عدم وجود تعارض في المواعيد
            var existingAvailabilities = await GetDoctorAvailabilitiesAsync(availability.DoctorId);
            var hasConflict = existingAvailabilities.Any(a =>
                a.DayOfWeek == availability.DayOfWeek &&
                ((availability.StartTime >= a.StartTime && availability.StartTime < a.EndTime) ||
                 (availability.EndTime > a.StartTime && availability.EndTime <= a.EndTime) ||
                 (availability.StartTime <= a.StartTime && availability.EndTime >= a.EndTime))
            );

            if (hasConflict)
                throw new InvalidOperationException("Availability schedule conflicts with existing schedule");

            await _unitOfWork.DoctorAvailabilities.AddAsync(availability);
            await _unitOfWork.SaveChangesAsync();
            return availability;
        }

        public async Task<DoctorAvailability> UpdateDoctorAvailabilityAsync(
            Guid doctorId,
            Guid availabilityId,
            SysDayOfWeek? dayOfWeek = null,
            TimeOnly? startTime = null,
            TimeOnly? endTime = null)
        {
            var availability = await _unitOfWork.DoctorAvailabilities
                .GetByIdAsync(availabilityId);

            if (availability == null || availability.DoctorId != doctorId)
                throw new ArgumentException("Availability not found");

            if (dayOfWeek.HasValue)
                availability.DayOfWeek = dayOfWeek.Value;

            if (startTime.HasValue)
                availability.StartTime = startTime.Value;

            if (endTime.HasValue)
                availability.EndTime = endTime.Value;

            // التحقق من صحة الأوقات
            if (availability.EndTime <= availability.StartTime)
                throw new InvalidOperationException("End time must be after start time");

            _unitOfWork.DoctorAvailabilities.Update(availability);
            await _unitOfWork.SaveChangesAsync();
            return availability;
        }

        public async Task<bool> DeleteDoctorAvailabilityAsync(Guid doctorId, Guid availabilityId)
        {
            var availability = await _unitOfWork.DoctorAvailabilities
                .GetByIdAsync(availabilityId);

            if (availability == null || availability.DoctorId != doctorId)
                return false;

            _unitOfWork.DoctorAvailabilities.Delete(availability);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // ==================== CONSULTATIONS ====================

        public async Task<IEnumerable<DoctorConsultation>> GetDoctorConsultationsAsync(Guid doctorId)
        {
            return await _unitOfWork.DoctorConsultations
                .FindAsync(c => c.DoctorId == doctorId);
        }

        public async Task<DoctorConsultation> AddDoctorConsultationAsync(
            DoctorConsultation consultation,
            ConsultationTypeEnum consultationType)
        {
            if (consultation == null)
                throw new ArgumentNullException(nameof(consultation));

            // البحث أو إنشاء ConsultationType
            var existingType = await _unitOfWork.ConsultationTypes
                .FindAsync(ct => ct.ConsultationTypeEnum == consultationType);

            var consultationTypeEntity = existingType.FirstOrDefault();
            if (consultationTypeEntity == null)
            {
                consultationTypeEntity = new ConsultationType
                {
                    ConsultationTypeEnum = consultationType
                };
                await _unitOfWork.ConsultationTypes.AddAsync(consultationTypeEntity);
                await _unitOfWork.SaveChangesAsync();
            }

            consultation.ConsultationTypeId = consultationTypeEntity.Id;

            await _unitOfWork.DoctorConsultations.AddAsync(consultation);
            await _unitOfWork.SaveChangesAsync();
            return consultation;
        }

        public async Task<DoctorConsultation> UpdateDoctorConsultationAsync(
            Guid doctorId,
            Guid consultationId,
            decimal? consultationFee = null,
            int? sessionDurationMinutes = null)
        {
            var consultation = await _unitOfWork.DoctorConsultations
                .GetByIdAsync(consultationId);

            if (consultation == null || consultation.DoctorId != doctorId)
                throw new ArgumentException("Consultation not found");

            if (consultationFee.HasValue)
                consultation.ConsultationFee = consultationFee.Value;

            if (sessionDurationMinutes.HasValue)
                consultation.SessionDurationMinutes = sessionDurationMinutes.Value;

            _unitOfWork.DoctorConsultations.Update(consultation);
            await _unitOfWork.SaveChangesAsync();
            return consultation;
        }

        public async Task<bool> DeleteDoctorConsultationAsync(Guid doctorId, Guid consultationId)
        {
            var consultation = await _unitOfWork.DoctorConsultations
                .GetByIdAsync(consultationId);

            if (consultation == null || consultation.DoctorId != doctorId)
                return false;

            _unitOfWork.DoctorConsultations.Delete(consultation);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // ==================== DOCUMENTS ====================

        public async Task<IEnumerable<DoctorDocument>> GetDoctorDocumentsAsync(Guid doctorId)
        {
            return await _unitOfWork.DoctorDocuments
                .FindAsync(d => d.DoctorId == doctorId);
        }

        public async Task<DoctorDocument> AddDoctorDocumentAsync(DoctorDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            await _unitOfWork.DoctorDocuments.AddAsync(document);
            await _unitOfWork.SaveChangesAsync();
            return document;
        }

        public async Task<bool> DeleteDoctorDocumentAsync(Guid doctorId, Guid documentId)
        {
            var document = await _unitOfWork.DoctorDocuments
                .GetByIdAsync(documentId);

            if (document == null || document.DoctorId != doctorId)
                return false;

            _unitOfWork.DoctorDocuments.Delete(document);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // ==================== OVERRIDES ====================

        public async Task<IEnumerable<DoctorOverride>> GetDoctorOverridesAsync(Guid doctorId)
        {
            return await _unitOfWork.DoctorOverrides
                .FindAsync(o => o.DoctorId == doctorId);
        }

        public async Task<DoctorOverride> AddDoctorOverrideAsync(DoctorOverride overrideSchedule)
        {
            if (overrideSchedule == null)
                throw new ArgumentNullException(nameof(overrideSchedule));

            await _unitOfWork.DoctorOverrides.AddAsync(overrideSchedule);
            await _unitOfWork.SaveChangesAsync();
            return overrideSchedule;
        }

        public async Task<bool> DeleteDoctorOverrideAsync(Guid doctorId, Guid overrideId)
        {
            var overrideSchedule = await _unitOfWork.DoctorOverrides
                .GetByIdAsync(overrideId);

            if (overrideSchedule == null || overrideSchedule.DoctorId != doctorId)
                return false;

            _unitOfWork.DoctorOverrides.Delete(overrideSchedule);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}