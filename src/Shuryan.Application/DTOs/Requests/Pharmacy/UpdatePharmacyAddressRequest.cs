using System.ComponentModel.DataAnnotations;
using Shuryan.Core.Enums;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    /// <summary>
    /// Request DTO لتحديث عنوان الصيدلية (Partial Update)
    /// كل الـ fields اختيارية - بيحدث بس الحاجات اللي انت بعتها
    /// </summary>
    public class UpdatePharmacyAddressRequest
    {
        /// <summary>
        /// المحافظة (اختياري)
        /// </summary>
        public Governorate? Governorate { get; set; }

        /// <summary>
        /// المدينة (اختياري)
        /// </summary>
        [StringLength(100, ErrorMessage = "اسم المدينة يجب ألا يتجاوز 100 حرف")]
        public string? City { get; set; }

        /// <summary>
        /// الشارع (اختياري)
        /// </summary>
        [StringLength(200, ErrorMessage = "اسم الشارع يجب ألا يتجاوز 200 حرف")]
        public string? Street { get; set; }

        /// <summary>
        /// رقم المبنى (اختياري)
        /// </summary>
        [StringLength(20, ErrorMessage = "رقم المبنى يجب ألا يتجاوز 20 حرف")]
        public string? BuildingNumber { get; set; }

        /// <summary>
        /// خط العرض (Latitude) (اختياري)
        /// </summary>
        [Range(-90, 90, ErrorMessage = "خط العرض يجب أن يكون بين -90 و 90")]
        public double? Latitude { get; set; }

        /// <summary>
        /// خط الطول (Longitude) (اختياري)
        /// </summary>
        [Range(-180, 180, ErrorMessage = "خط الطول يجب أن يكون بين -180 و 180")]
        public double? Longitude { get; set; }
    }
}
