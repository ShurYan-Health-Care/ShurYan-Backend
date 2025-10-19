using Shuryan.Application.Interfaces;
using Shuryan.Core.Interfaces.Services;
using Shuryan.Application.DTOs.Requests.Doctor;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.Mappers;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical.Schedules;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{
    public class DoctorApplicationService : IDoctorApplicationService
    {
        private readonly IDoctorService _doctorService;

        public DoctorApplicationService(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        // ==================== DOCTOR CRUD ====================

        public async Task<DoctorResponse?> GetDoctorByIdAsync(Guid id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            return doctor != null ? DoctorMapper.MapToDoctorResponse(doctor) : null;
        }

        public async Task<DoctorResponse?> GetDoctorByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            var doctor = await _doctorService.GetDoctorByEmailAsync(email);
            return doctor != null ? DoctorMapper.MapToDoctorResponse(doctor) : null;
        }

        public async Task<IEnumerable<DoctorResponse>> GetDoctorsBySpecialtyAsync(MedicalSpecialty specialty)
        {
            var doctors = await _doctorService.GetDoctorsBySpecialtyAsync(specialty);
            return DoctorMapper.MapToDoctorResponseList(doctors);
        }

        public async Task<IEnumerable<DoctorResponse>> GetVerifiedDoctorsAsync()
        {
            var doctors = await _doctorService.GetVerifiedDoctorsAsync();
            return DoctorMapper.MapToDoctorResponseList(doctors);
        }

        public async Task<IEnumerable<DoctorResponse>> GetDoctorsByGovernorateAsync(Governorate governorate)
        {
            var doctors = await _doctorService.GetDoctorsByGovernorateAsync(governorate);
            return DoctorMapper.MapToDoctorResponseList(doctors);
        }

        public async Task<PaginatedResponse<DoctorResponse>> SearchDoctorsAsync(SearchDoctorsRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var doctors = await _doctorService.SearchDoctorsAsync(
                request.SearchTerm,
                request.Specialty,
                request.Governorate,
                request.MinYearsOfExperience,
                request.MaxConsultationFee,
                request.MinRating
            );

            var totalCount = doctors.Count();
            var paginatedDoctors = doctors
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

            return new PaginatedResponse<DoctorResponse>
            {
                Data = DoctorMapper.MapToDoctorResponseList(paginatedDoctors),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        public async Task<bool> IsDoctorAvailableAtAsync(Guid doctorId, DateTime dateTime)
        {
            return await _doctorService.IsDoctorAvailableAtAsync(doctorId, dateTime);
        }

        public async Task<DoctorResponse> CreateDoctorAsync(CreateDoctorRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // التحقق من عدم وجود دكتور بنفس الإيميل
            var existingDoctor = await _doctorService.GetDoctorByEmailAsync(request.Email);
            if (existingDoctor != null)
                throw new InvalidOperationException($"Doctor with email {request.Email} already exists");

            var doctor = new Doctor
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email, // مهم للـ Identity
                PhoneNumber = request.PhoneNumber,
                MedicalSpecialty = request.MedicalSpecialty,
                YearsOfExperience = request.YearsOfExperience,
                Biography = request.Biography,
                ProfileImageUrl = request.ProfileImageUrl,
                BirthDate = request.BirthDate,
                Gender = request.Gender
            };

            var createdDoctor = await _doctorService.CreateDoctorAsync(doctor);
            return DoctorMapper.MapToDoctorResponse(createdDoctor);
        }

        public async Task<DoctorResponse> UpdateDoctorAsync(Guid id, UpdateDoctorRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // التحقق من وجود الدكتور
            var existingDoctor = await _doctorService.GetDoctorByIdAsync(id);
            if (existingDoctor == null)
                throw new ArgumentException($"Doctor with ID {id} not found");

            // تحديث البيانات فقط اللي موجودة في الـ Request
            if (!string.IsNullOrWhiteSpace(request.FirstName))
                existingDoctor.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                existingDoctor.LastName = request.LastName;

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                existingDoctor.PhoneNumber = request.PhoneNumber;

            if (request.MedicalSpecialty.HasValue)
                existingDoctor.MedicalSpecialty = request.MedicalSpecialty.Value;

            if (request.YearsOfExperience.HasValue)
                existingDoctor.YearsOfExperience = request.YearsOfExperience.Value;

            if (!string.IsNullOrWhiteSpace(request.Biography))
                existingDoctor.Biography = request.Biography;

            if (!string.IsNullOrWhiteSpace(request.ProfileImageUrl))
                existingDoctor.ProfileImageUrl = request.ProfileImageUrl;

            if (request.BirthDate.HasValue)
                existingDoctor.BirthDate = request.BirthDate;

            if (request.Gender.HasValue)
                existingDoctor.Gender = request.Gender;

            var updatedDoctor = await _doctorService.UpdateDoctorAsync(id, existingDoctor);
            return DoctorMapper.MapToDoctorResponse(updatedDoctor);
        }

        public async Task<bool> DeleteDoctorAsync(Guid id)
        {
            return await _doctorService.DeleteDoctorAsync(id);
        }

        public async Task<bool> VerifyDoctorAsync(Guid id, VerifyDoctorRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var status = request.IsVerified ? VerificationStatus.Verified : VerificationStatus.Rejected;
            return await _doctorService.VerifyDoctorAsync(id, status, request.VerifierId);
        }

        // ==================== AVAILABILITY ====================

        public async Task<IEnumerable<DoctorAvailabilityResponse>> GetDoctorAvailabilitiesAsync(Guid doctorId)
        {
            var availabilities = await _doctorService.GetDoctorAvailabilitiesAsync(doctorId);
            return availabilities.Select(DoctorMapper.MapToAvailabilityResponse);
        }

        public async Task<DoctorAvailabilityResponse> AddDoctorAvailabilityAsync(
            Guid doctorId,
            CreateDoctorAvailabilityRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // التحقق من وجود الدكتور
            var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
            if (doctor == null)
                throw new ArgumentException($"Doctor with ID {doctorId} not found");

            // التحقق من صحة الأوقات
            if (request.EndTime <= request.StartTime)
                throw new InvalidOperationException("End time must be after start time");

            // تحويل string إلى SysDayOfWeek enum
            if (!Enum.TryParse<SysDayOfWeek>(request.DayOfWeek, true, out var dayOfWeek))
                throw new ArgumentException($"Invalid day of week: {request.DayOfWeek}");

            var availability = new DoctorAvailability
            {
                DoctorId = doctorId,
                DayOfWeek = dayOfWeek,
                StartTime = TimeOnly.FromTimeSpan(request.StartTime),
                EndTime = TimeOnly.FromTimeSpan(request.EndTime)
            };

            var created = await _doctorService.AddDoctorAvailabilityAsync(availability);
            return DoctorMapper.MapToAvailabilityResponse(created);
        }

        public async Task<DoctorAvailabilityResponse> UpdateDoctorAvailabilityAsync(
            Guid doctorId,
            Guid availabilityId,
            UpdateDoctorAvailabilityRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var updated = await _doctorService.UpdateDoctorAvailabilityAsync(
                doctorId,
                availabilityId,
                request.DayOfWeek,
                request.StartTime.HasValue ? TimeOnly.FromTimeSpan(request.StartTime.Value) : null,
                request.EndTime.HasValue ? TimeOnly.FromTimeSpan(request.EndTime.Value) : null
            );

            return DoctorMapper.MapToAvailabilityResponse(updated);
        }

        public async Task<bool> DeleteDoctorAvailabilityAsync(Guid doctorId, Guid availabilityId)
        {
            return await _doctorService.DeleteDoctorAvailabilityAsync(doctorId, availabilityId);
        }

        // ==================== CONSULTATIONS ====================

        public async Task<IEnumerable<DoctorConsultationResponse>> GetDoctorConsultationsAsync(Guid doctorId)
        {
            var consultations = await _doctorService.GetDoctorConsultationsAsync(doctorId);
            return consultations.Select(DoctorMapper.MapToConsultationResponse);
        }

        public async Task<DoctorConsultationResponse> AddDoctorConsultationAsync(
            Guid doctorId,
            CreateDoctorConsultationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // التحقق من وجود الدكتور
            var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
            if (doctor == null)
                throw new ArgumentException($"Doctor with ID {doctorId} not found");

            // التحقق من عدم وجود نفس نوع الكشف
            var existingConsultations = await _doctorService.GetDoctorConsultationsAsync(doctorId);
            if (existingConsultations.Any(c => c.ConsultationType.ConsultationTypeEnum == request.ConsultationType))
                throw new InvalidOperationException($"Consultation type {request.ConsultationType} already exists for this doctor");

            var consultation = new DoctorConsultation
            {
                DoctorId = doctorId,
                ConsultationFee = request.ConsultationFee,
                SessionDurationMinutes = request.SessionDurationMinutes,
                // سيتم ربط ConsultationType من خلال ConsultationTypeId
                // يجب إنشاء/جلب ConsultationType أولاً
            };

            var created = await _doctorService.AddDoctorConsultationAsync(consultation, request.ConsultationType);
            return DoctorMapper.MapToConsultationResponse(created);
        }

        public async Task<DoctorConsultationResponse> UpdateDoctorConsultationAsync(
            Guid doctorId,
            Guid consultationId,
            UpdateDoctorConsultationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var updated = await _doctorService.UpdateDoctorConsultationAsync(
                doctorId,
                consultationId,
                request.ConsultationFee,
                request.SessionDurationMinutes
            );

            return DoctorMapper.MapToConsultationResponse(updated);
        }

        public async Task<bool> DeleteDoctorConsultationAsync(Guid doctorId, Guid consultationId)
        {
            return await _doctorService.DeleteDoctorConsultationAsync(doctorId, consultationId);
        }

        // ==================== DOCUMENTS ====================

        public async Task<IEnumerable<DoctorDocumentResponse>> GetDoctorDocumentsAsync(Guid doctorId)
        {
            var documents = await _doctorService.GetDoctorDocumentsAsync(doctorId);
            return documents.Select(DoctorMapper.MapToDocumentResponse);
        }

        public async Task<DoctorDocumentResponse> AddDoctorDocumentAsync(
            Guid doctorId,
            CreateDoctorDocumentRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // التحقق من وجود الدكتور
            var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
            if (doctor == null)
                throw new ArgumentException($"Doctor with ID {doctorId} not found");

            var document = new DoctorDocument
            {
                DoctorId = doctorId,
                DocumentUrl = request.DocumentUrl,
                Type = request.Type,
                Status = VerificationDocumentStatus.Pending // دائماً يبدأ Pending
            };

            var created = await _doctorService.AddDoctorDocumentAsync(document);
            return DoctorMapper.MapToDocumentResponse(created);
        }

        public async Task<bool> DeleteDoctorDocumentAsync(Guid doctorId, Guid documentId)
        {
            return await _doctorService.DeleteDoctorDocumentAsync(doctorId, documentId);
        }

        // ==================== OVERRIDES ====================

        public async Task<IEnumerable<DoctorOverrideResponse>> GetDoctorOverridesAsync(Guid doctorId)
        {
            var overrides = await _doctorService.GetDoctorOverridesAsync(doctorId);
            return overrides.Select(DoctorMapper.MapToOverrideResponse);
        }

        public async Task<DoctorOverrideResponse> AddDoctorOverrideAsync(
            Guid doctorId,
            CreateDoctorOverrideRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // التحقق من وجود الدكتور
            var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
            if (doctor == null)
                throw new ArgumentException($"Doctor with ID {doctorId} not found");

            // التحقق من صحة الأوقات
            if (request.EndTime <= request.StartTime)
                throw new InvalidOperationException("End time must be after start time");

            var overrideSchedule = new DoctorOverride
            {
                DoctorId = doctorId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Type = request.Type
            };

            var created = await _doctorService.AddDoctorOverrideAsync(overrideSchedule);
            return DoctorMapper.MapToOverrideResponse(created);
        }

        public async Task<bool> DeleteDoctorOverrideAsync(Guid doctorId, Guid overrideId)
        {
            return await _doctorService.DeleteDoctorOverrideAsync(doctorId, overrideId);
        }
    }
}