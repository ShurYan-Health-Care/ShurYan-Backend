namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    /// <summary>
    /// Response DTO للمعلومات الأساسية للصيدلية
    /// </summary>
    public class PharmacyBasicInfoResponse
    {
        /// <summary>
        /// اسم الصيدلية
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// البريد الإلكتروني
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// رقم الهاتف
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// صورة البروفايل
        /// </summary>
        public string? ProfileImageUrl { get; set; }
    }
}
