using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class UpdateDoctorRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Biography { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        public MedicalSpecialty? MedicalSpecialty { get; set; }
    }
}

