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
        
        public MedicalSpecialty? MedicalSpecialty { get; set; }

        public Governorate? Governorate { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        [Range(0, 5, ErrorMessage = "Minimum rating must be between 0-5")]
        public double? MinRating { get; set; }

        [Range(0, 100000, ErrorMessage = "Minimum price must be between 0-100000")]
        public decimal? MinPrice { get; set; }

        [Range(0, 100000, ErrorMessage = "Maximum price must be between 0-100000")]
        public decimal? MaxPrice { get; set; }

        public bool? AvailableToday { get; set; }
    }
}

