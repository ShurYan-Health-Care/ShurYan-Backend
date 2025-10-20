using Shuryan.Application.DTOs.Responses.Clinic;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.External.Clinic;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Entities.Medical.Schedules;
using System.Collections.Generic;
using System.Linq;

namespace Shuryan.Application.Mappers
{
    public static class DoctorMapper
    {
        // ==================== DOCTOR MAPPING ====================

        public static DoctorResponse MapToDoctorResponse(Doctor doctor)
        {
            if (doctor == null)
                throw new ArgumentNullException(nameof(doctor));

            return new DoctorResponse
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Email = doctor.Email,
                PhoneNumber = doctor.PhoneNumber,
                MedicalSpecialty = doctor.MedicalSpecialty,
                YearsOfExperience = doctor.YearsOfExperience,
                Biography = doctor.Biography,
                VerificationStatus = doctor.VerificationStatus,
                VerifiedAt = doctor.VerifiedAt,
                VerifierId = doctor.VerifierId,
                ProfileImageUrl = doctor.ProfileImageUrl,
                BirthDate = doctor.BirthDate,
                Gender = doctor.Gender,
                CreatedAt = doctor.CreatedAt,
                UpdatedAt = doctor.UpdatedAt,

                // Map related entities if loaded
                Clinic = doctor.Clinic != null ? MapToClinicResponse(doctor.Clinic) : null,
                Consultations = doctor.Consultations?.Select(MapToConsultationResponse).ToList()
                    ?? new List<DoctorConsultationResponse>(),
                Availabilities = doctor.Availabilities?.Select(MapToAvailabilityResponse).ToList()
                    ?? new List<DoctorAvailabilityResponse>(),
                VerificationDocuments = doctor.VerificationDocuments?.Select(MapToDocumentResponse).ToList()
                    ?? new List<DoctorDocumentResponse>(),

                // Calculate ratings
                AverageRating = doctor.DoctorReviews?.Any() == true
                    ? doctor.DoctorReviews.Average(r => r.AverageRating)
                    : null,
                TotalReviewsCount = doctor.DoctorReviews?.Count ?? 0
            };
        }

        public static IEnumerable<DoctorResponse> MapToDoctorResponseList(IEnumerable<Doctor> doctors)
        {
            return doctors?.Select(MapToDoctorResponse) ?? Enumerable.Empty<DoctorResponse>();
        }

        // ==================== AVAILABILITY MAPPING ====================

        public static DoctorAvailabilityResponse MapToAvailabilityResponse(DoctorAvailability availability)
        {
            if (availability == null)
                throw new ArgumentNullException(nameof(availability));

            return new DoctorAvailabilityResponse
            {
                Id = availability.Id,
                DoctorId = availability.DoctorId,
                DayOfWeek = availability.DayOfWeek,
                StartTime = availability.StartTime.ToTimeSpan(),
                EndTime = availability.EndTime.ToTimeSpan(),
                CreatedAt = availability.CreatedAt,
                UpdatedAt = availability.UpdatedAt,
                IsDeleted = availability.IsDeleted,
                DeletedAt = availability.DeletedAt
            };
        }

        // ==================== CONSULTATION MAPPING ====================

        public static DoctorConsultationResponse MapToConsultationResponse(DoctorConsultation consultation)
        {
            if (consultation == null)
                throw new ArgumentNullException(nameof(consultation));

            return new DoctorConsultationResponse
            {
                DoctorId = consultation.DoctorId,
                ConsultationType = consultation.ConsultationType?.ConsultationTypeEnum ?? default,
                ConsultationFee = consultation.ConsultationFee,
                SessionDurationMinutes = consultation.SessionDurationMinutes,
            };
        }

        // ==================== DOCUMENT MAPPING ====================

        public static DoctorDocumentResponse MapToDocumentResponse(DoctorDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            return new DoctorDocumentResponse
            {
                Id = document.Id,
                DoctorId = document.DoctorId,
                DocumentUrl = document.DocumentUrl,
                Type = document.Type,
                Status = document.Status,
                RejectionReason = document.RejectionReason,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt
            };
        }

        // ==================== OVERRIDE MAPPING ====================

        public static DoctorOverrideResponse MapToOverrideResponse(DoctorOverride overrideSchedule)
        {
            if (overrideSchedule == null)
                throw new ArgumentNullException(nameof(overrideSchedule));

            return new DoctorOverrideResponse
            {
                Id = overrideSchedule.Id,
                DoctorId = overrideSchedule.DoctorId,
                StartTime = overrideSchedule.StartTime,
                EndTime = overrideSchedule.EndTime,
                Type = overrideSchedule.Type,
                CreatedAt = overrideSchedule.CreatedAt,
                UpdatedAt = overrideSchedule.UpdatedAt
            };
        }

        // ==================== CLINIC MAPPING (Helper) ====================

        private static ClinicResponse? MapToClinicResponse(Clinic clinic)
        {
            if (clinic == null)
                return null;

            return new ClinicResponse
            {
                Id = clinic.Id,
                Name = clinic.Name
            };
        }
    }
}