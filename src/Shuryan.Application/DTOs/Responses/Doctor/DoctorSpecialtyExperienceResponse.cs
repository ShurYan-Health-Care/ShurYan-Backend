using Shuryan.Core.Enums.Doctor;
using System;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    /// <summary>
    /// Response DTO for doctor's specialty and years of experience
    /// التخصص وسنوات الخبرة للدكتور
    /// </summary>
    public class DoctorSpecialtyExperienceResponse
    {
        public Guid DoctorId { get; set; }
        public MedicalSpecialty MedicalSpecialty { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
    }
}
