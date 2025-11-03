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
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "رقم الهاتف يجب أن يكون بين 10-20 رقم")]
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
    }
}

