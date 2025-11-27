using Shuryan.Core.Enums.Doctor;
using System;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    public class DoctorListItemResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"د. {FirstName} {LastName}";
        
        public MedicalSpecialty MedicalSpecialty { get; set; }
        public string MedicalSpecialtyName { get; set; } = string.Empty;
        
        public string Governorate { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        
        // أقرب وقت متاح للحجز
        public DateTime? NextAvailableSlot { get; set; }
        
        // متوسط التقييم
        public double? AverageRating { get; set; }
        
        // سعر الكشف العادي
        public decimal RegularConsultationFee { get; set; }
        
        // صورة البروفايل
        public string? ProfileImageUrl { get; set; }
    }
}
