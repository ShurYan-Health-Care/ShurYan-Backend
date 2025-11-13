namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    /// <summary>
    /// Response DTO لإعدادات التوصيل للصيدلية
    /// </summary>
    public class PharmacyDeliveryResponse
    {
        /// <summary>
        /// هل الصيدلية بتوفر خدمة التوصيل؟
        /// </summary>
        public bool OffersDelivery { get; set; }

        /// <summary>
        /// سعر التوصيل (0 للتوصيل المجاني)
        /// </summary>
        public decimal DeliveryFee { get; set; }
    }
}
