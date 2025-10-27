using Microsoft.AspNetCore.Http;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class UpdateDoctorRequest
    {
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2-50 characters")]
        public string? FirstName { get; set; }

        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2-50 characters")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10-20 characters")]
        public string? PhoneNumber { get; set; }

        [Range(0, 70, ErrorMessage = "Years of experience must be between 0-70")]
        public int? YearsOfExperience { get; set; }

        [StringLength(5000, ErrorMessage = "Biography cannot exceed 5000 characters")]
        public string? Biography { get; set; }

        public IFormFile? ProfileImage { get; set; }

        public DateTime? BirthDate { get; set; }

        public Gender? Gender { get; set; }

        public MedicalSpecialty? MedicalSpecialty { get; set; }
    }
}

