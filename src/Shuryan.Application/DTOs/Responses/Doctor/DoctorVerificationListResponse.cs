using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    /// <summary>
    /// Response DTO لقائمة الأطباء مع حالة التحقق والمستندات
    /// </summary>
    public class DoctorVerificationListResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"د. {FirstName} {LastName}";
        
        // التخصص الطبي
        public MedicalSpecialty MedicalSpecialty { get; set; }
        public string MedicalSpecialtyName { get; set; } = string.Empty;
        
        // العنوان
        public string Governorate { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        
        // صورة البروفايل
        public string? ProfileImageUrl { get; set; }
        
        // سنوات الخبرة
        public int? YearsOfExperience { get; set; }
        
        // معلومات الاتصال
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        
        // حالة التحقق
        public VerificationStatus VerificationStatus { get; set; }
        public string VerificationStatusName { get; set; } = string.Empty;
        
        // المستندات
        public List<DoctorDocumentItemResponse> Documents { get; set; } = new List<DoctorDocumentItemResponse>();
    }

    /// <summary>
    /// Response DTO لمستند واحد من مستندات الدكتور
    /// </summary>
    public class DoctorDocumentItemResponse
    {
        public Guid Id { get; set; }
        public string DocumentUrl { get; set; } = string.Empty;
        public DoctorDocumentType Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public VerificationDocumentStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
