using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    /// <summary>
    /// Request DTO لتحديث إعدادات التوصيل للصيدلية
    /// </summary>
    public class UpdatePharmacyDeliveryRequest
    {
        /// <summary>
        /// هل الصيدلية بتوفر خدمة التوصيل؟
        /// </summary>
        [Required(ErrorMessage = "حالة التوصيل مطلوبة")]
        public bool OffersDelivery { get; set; }

        /// <summary>
        /// سعر التوصيل (0 للتوصيل المجاني)
        /// </summary>
        [Range(0, 1000, ErrorMessage = "سعر التوصيل يجب أن يكون بين 0 و 1000")]
        public decimal DeliveryFee { get; set; } = 0;
    }
}
