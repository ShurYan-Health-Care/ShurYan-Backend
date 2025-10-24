using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class SearchDoctorsRequest : PaginationParams
    {
        [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string? SearchTerm { get; set; }

        public MedicalSpecialty? Specialty { get; set; }

        public Governorate? Governorate { get; set; }

        [Range(0, 30, ErrorMessage = "Minimum years of experience must be between 0-70")]
        public int? MinYearsOfExperience { get; set; }

        [Range(0, 10000, ErrorMessage = "Maximum consultation fee must be between 0-10000")]
        public decimal? MaxConsultationFee { get; set; }

        [Range(0, 5, ErrorMessage = "Minimum rating must be between 0-5")]
        public double? MinRating { get; set; }
    }
}

