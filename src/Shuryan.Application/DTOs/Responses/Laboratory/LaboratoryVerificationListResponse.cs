using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    /// <summary>
    /// Response DTO لقائمة المعامل مع حالة التحقق والمستندات - تُستخدم في لوحة المراجع
    /// </summary>
    public class LaboratoryVerificationListResponse
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
        public List<LaboratoryDocumentItemResponse> Documents { get; set; } = new List<LaboratoryDocumentItemResponse>();
    }

    /// <summary>
    /// Response DTO لمستند واحد من مستندات المعمل
    /// </summary>
    public class LaboratoryDocumentItemResponse
    {
        public Guid Id { get; set; }
        public string DocumentUrl { get; set; } = string.Empty;
        public LaboratoryDocumentType Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public VerificationDocumentStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
