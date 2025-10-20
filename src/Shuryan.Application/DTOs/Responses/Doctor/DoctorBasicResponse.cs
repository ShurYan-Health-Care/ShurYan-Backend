using Shuryan.Core.Enums.Doctor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    public class DoctorBasicResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public MedicalSpecialty MedicalSpecialty { get; set; }
        public string? ProfileImageUrl { get; set; }
        public double? AverageRating { get; set; }
        public int YearsOfExperience { get; set; }
    }
}

