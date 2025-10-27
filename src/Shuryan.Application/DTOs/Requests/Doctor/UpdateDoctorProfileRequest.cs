using Microsoft.AspNetCore.Http;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class UpdateDoctorProfileRequest
    {
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        public IFormFile? ProfileImage { get; set; }

        public Gender? Gender { get; set; }
        
        public DateTime? DateOfBirth { get; set; }

        public MedicalSpecialty? MedicalSpecialty { get; set; }

        [Range(0, 70, ErrorMessage = "Years of experience must be between 0 and 70")]
        public int? YearsOfExperience { get; set; }

        [StringLength(2000, ErrorMessage = "Biography cannot exceed 2000 characters")]
        public string? Biography { get; set; }
    }
}
