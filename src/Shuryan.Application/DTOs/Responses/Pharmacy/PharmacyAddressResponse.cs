using Shuryan.Core.Enums;

namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    /// <summary>
    /// Response DTO لعنوان الصيدلية
    /// </summary>
    public class PharmacyAddressResponse
    {
        /// <summary>
        /// المحافظة
        /// </summary>
        public Governorate Governorate { get; set; }

        /// <summary>
        /// المدينة
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// الشارع
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// رقم المبنى
        /// </summary>
        public string? BuildingNumber { get; set; }

        /// <summary>
        /// خط العرض (Latitude)
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// خط الطول (Longitude)
        /// </summary>
        public double? Longitude { get; set; }
    }
}
