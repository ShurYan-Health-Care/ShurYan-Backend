using Shuryan.Application.DTOs.Base;
using Shuryan.Core.Enums.Doctor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Doctor
{
    public class DoctorSearchFilterDto : PaginationParams
    {
        public string? SearchTerm { get; set; }
        public MedicalSpecialty? Specialty { get; set; }
        public string? Governorate { get; set; }
        public int? MinYearsOfExperience { get; set; }
        public decimal? MaxConsultationFee { get; set; }
        public double? MinRating { get; set; }
    }

}
