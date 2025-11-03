using Shuryan.Application.DTOs.Responses.Clinic;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    /// <summary>
    /// Response DTO لتفاصيل الدكتور الكاملة مع معلومات العيادة
    /// </summary>
    public class DoctorDetailsWithClinicResponse
    {
        // معلومات الدكتور الأساسية
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"د. {FirstName} {LastName}";
        
        // التخصص الطبي
        public MedicalSpecialty MedicalSpecialty { get; set; }
        public string MedicalSpecialtyName { get; set; } = string.Empty;
        
        // الجنس
        public Gender? Gender { get; set; }
        public string? GenderName { get; set; }
        
        // تاريخ الميلاد
        public DateTime? DateOfBirth { get; set; }
        
        // صورة البروفايل
        public string? ProfileImageUrl { get; set; }
        
        // البايو
        public string? Biography { get; set; }
        
        // عدد سنين الخبرة
        public int YearsOfExperience { get; set; }
        
        // معلومات العيادة
        public ClinicDetailsResponse? Clinic { get; set; }
        
        // الشركاء المقترحين
        public PartnerSuggestionsResponse? PartnerSuggestions { get; set; }
        
        // متوسط التقييم
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
    
    /// <summary>
    /// معلومات العيادة التفصيلية
    /// </summary>
    public class ClinicDetailsResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        // أرقام الهواتف
        public IEnumerable<ClinicPhoneNumberResponse> PhoneNumbers { get; set; } = new List<ClinicPhoneNumberResponse>();
        
        // الخدمات المتاحة
        public IEnumerable<ClinicServiceResponse> OfferedServices { get; set; } = new List<ClinicServiceResponse>();
        
        // العنوان الكامل
        public ClinicAddressResponse? Address { get; set; }
        
        // صور العيادة
        public IEnumerable<ClinicPhotoResponse> Photos { get; set; } = new List<ClinicPhotoResponse>();
    }
    
    /// <summary>
    /// الشركاء المقترحين (صيدلية ومعمل)
    /// </summary>
    public class PartnerSuggestionsResponse
    {
        public PartnerBasicInfoResponse? SuggestedPharmacy { get; set; }
        public PartnerBasicInfoResponse? SuggestedLaboratory { get; set; }
    }
    
    /// <summary>
    /// معلومات أساسية عن الشريك (صيدلية أو معمل)
    /// </summary>
    public class PartnerBasicInfoResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
