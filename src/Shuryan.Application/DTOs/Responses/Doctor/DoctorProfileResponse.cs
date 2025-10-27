using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using System;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    public class DoctorProfileResponse : BaseAuditableDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public Gender Gender { get; set; }
        public string GenderName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public MedicalSpecialty MedicalSpecialty { get; set; }
        public string MedicalSpecialtyName { get; set; } = string.Empty;
        public string? Biography { get; set; }
        

        // Verification
        public VerificationStatus VerificationStatus { get; set; }
        public string VerificationStatusName { get; set; } = string.Empty;
    }
}
