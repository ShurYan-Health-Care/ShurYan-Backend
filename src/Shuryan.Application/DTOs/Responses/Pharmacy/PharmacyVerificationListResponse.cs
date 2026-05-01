using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;


namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    /// <summary>
    /// Response DTO لقائمة الصيدليات مع حالة التحقق والمستندات - تُستخدم في لوحة المراجع
    /// </summary>
    public class PharmacyVerificationListResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullName => Name; // Alias to match doctor pattern in ApplicationCard

        // معلومات المالك
        public string? OwnerName { get; set; }

        // العنوان
        public string Governorate { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Address { get; set; }

        // صورة البروفايل
        public string? ProfileImageUrl { get; set; }

        // معلومات الاتصال
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        // حالة التحقق
        public VerificationStatus VerificationStatus { get; set; }
        public string VerificationStatusName { get; set; } = string.Empty;

        // المستندات
        public List<PharmacyDocumentItemResponse> Documents { get; set; } = new List<PharmacyDocumentItemResponse>();
    }

    /// <summary>
    /// Response DTO لمستند واحد من مستندات الصيدلية
    /// </summary>
    public class PharmacyDocumentItemResponse
    {
        public Guid Id { get; set; }
        public string DocumentUrl { get; set; } = string.Empty;
        public PharmacyDocumentType Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public VerificationDocumentStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
