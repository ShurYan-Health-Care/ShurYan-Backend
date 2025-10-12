using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Core.Enums.Doctor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class DoctorSearchRequest : PaginationParams
    {
        public string? SearchTerm { get; set; }
        public MedicalSpecialty? Specialty { get; set; }
        public string? Governorate { get; set; }
        public int? MinYearsOfExperience { get; set; }
        public decimal? MaxConsultationFee { get; set; }
        public double? MinRating { get; set; }
    }
}

