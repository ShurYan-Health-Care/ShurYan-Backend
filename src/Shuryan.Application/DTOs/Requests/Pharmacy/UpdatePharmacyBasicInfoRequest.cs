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
        [RegularExpression("^01[0125][0-9]{8}$", ErrorMessage = "رقم الهاتف يجب أن يكون رقم مصري صحيح يتكون من 11 رقم ويبدأ بـ 010 أو 011 أو 012 أو 015")]
        public string? PhoneNumber { get; set; }
    }
}
