using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    /// <summary>
    /// Request DTO لتحديث المعلومات الأساسية للصيدلية (Partial Update)
    /// كل الـ fields اختيارية - بيحدث بس الحاجات اللي انت بعتها
    /// </summary>
    public class UpdatePharmacyBasicInfoRequest
    {
        /// <summary>
        /// اسم الصيدلية (اختياري)
        /// </summary>
        [StringLength(100, MinimumLength = 2, ErrorMessage = "اسم الصيدلية يجب أن يكون بين 2-100 حرف")]
        public string? Name { get; set; }

        /// <summary>
        /// رقم الهاتف (اختياري)
        /// </summary>
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "رقم الهاتف يجب أن يكون بين 10-20 رقم")]
        public string? PhoneNumber { get; set; }
    }
}
