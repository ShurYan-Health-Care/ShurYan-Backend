namespace Shuryan.Application.DTOs.Responses.Appointment
{
    /// <summary>
    /// Response DTO لأسعار ومدة الخدمات الطبية للدكتور
    /// </summary>
    public class DoctorServicesResponse
    {
        /// <summary>
        /// معلومات الكشف العادي
        /// </summary>
        public ServiceDetailsResponse RegularCheckup { get; set; } = new ServiceDetailsResponse();

        /// <summary>
        /// معلومات إعادة الكشف
        /// </summary>
        public ServiceDetailsResponse ReExamination { get; set; } = new ServiceDetailsResponse();
    }

    /// <summary>
    /// تفاصيل خدمة طبية واحدة
    /// </summary>
    public class ServiceDetailsResponse
    {
        /// <summary>
        /// السعر بالجنيه
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// المدة بالدقائق
        /// </summary>
        public int Duration { get; set; }
    }
}
