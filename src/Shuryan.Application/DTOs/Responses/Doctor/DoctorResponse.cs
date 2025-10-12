using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Responses.Clinic;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    public class DoctorResponse : BaseAuditableDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public MedicalSpecialty MedicalSpecialty { get; set; }
        public int YearsOfExperience { get; set; }
        public string? Biography { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifierId { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        public ClinicResponse? Clinic { get; set; }
        public IEnumerable<DoctorConsultationResponse> Consultations { get; set; } = new List<DoctorConsultationResponse>();
        public IEnumerable<DoctorAvailabilityResponse> Availabilities { get; set; } = new List<DoctorAvailabilityResponse>();
        public IEnumerable<DoctorDocumentResponse> VerificationDocuments { get; set; } = new List<DoctorDocumentResponse>();
        public double? AverageRating { get; set; }
        public int TotalReviewsCount { get; set; }
    }
}

