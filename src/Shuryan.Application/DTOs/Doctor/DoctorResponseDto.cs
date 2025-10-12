using Shuryan.Application.DTOs.Base;
using Shuryan.Application.DTOs.Clinic;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Doctor
{
    public class DoctorResponseDto : BaseAuditableDto
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
        public ClinicDto? Clinic { get; set; }
        public IEnumerable<DoctorConsultationDto> Consultations { get; set; } = new List<DoctorConsultationDto>();
        public IEnumerable<DoctorAvailabilityDto> Availabilities { get; set; } = new List<DoctorAvailabilityDto>();
        public IEnumerable<DoctorDocumentDto> VerificationDocuments { get; set; } = new List<DoctorDocumentDto>();
        public double? AverageRating { get; set; }
        public int TotalReviewsCount { get; set; }
    }

}
