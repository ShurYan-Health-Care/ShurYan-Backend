using Shuryan.Core.Enums.Doctor;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class UpdateSpecialtyExperienceRequest
    {
        [Required(ErrorMessage = "Medical specialty is required")]
        public MedicalSpecialty MedicalSpecialty { get; set; }

        [Required(ErrorMessage = "Years of experience is required")]
        [Range(0, 70, ErrorMessage = "Years of experience must be between 0 and 70")]
        public int YearsOfExperience { get; set; }
    }
}
