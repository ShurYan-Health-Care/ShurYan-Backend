using System;
using System.ComponentModel.DataAnnotations;
using Shuryan.Core.Enums.Identity;

namespace Shuryan.Application.DTOs.Requests.Patient
{
    /// <summary>
    /// Request DTO لتحديث البيانات الشخصية للمريض (Partial Update)
    /// كل الـ fields اختيارية - بيحدث بس الحاجات اللي انت بعتها
    /// 
    /// ملاحظة: الـ ProfileImage والـ Address ليهم endpoints منفصلة:
    /// - PUT /api/patients/me/profile-image (للصورة الشخصية)
    /// - PUT /api/patients/me/address (للعنوان)
    /// </summary>
    public class UpdatePatientRequest
    {
        /// <summary>
        /// الاسم الأول (اختياري)
        /// </summary>
        [StringLength(50, MinimumLength = 2, ErrorMessage = "الاسم الأول يجب أن يكون بين 2-50 حرف")]
        public string? FirstName { get; set; }

        /// <summary>
        /// الاسم الأخير (اختياري)
        /// </summary>
        [StringLength(50, MinimumLength = 2, ErrorMessage = "الاسم الأخير يجب أن يكون بين 2-50 حرف")]
        public string? LastName { get; set; }

        /// <summary>
        /// رقم الهاتف (اختياري)
        /// لو اتغير، هيتم reset للـ PhoneNumberConfirmed
        /// </summary>
        [RegularExpression("^01[0125][0-9]{8}$", ErrorMessage = "رقم الهاتف يجب أن يكون رقم مصري صحيح يتكون من 11 رقم ويبدأ بـ 010 أو 011 أو 012 أو 015")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// تاريخ الميلاد (اختياري)
        /// لازم يكون في الماضي، مش في المستقبل
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// النوع (ذكر/أنثى) (اختياري)
        /// </summary>
        public Gender? Gender { get; set; }

        // Emergency SOS & Medical Profile Fields
        public Shuryan.Core.Enums.Medical.BloodType? BloodType { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? HeightCm { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public bool? IsPregnant { get; set; }
        public string? PhysicalDisabilities { get; set; }
    }
}

