namespace Shuryan.Application.DTOs.Responses.Appointment
{
    /// <summary>
    /// Response DTO لفترة زمنية واحدة (اختياري - للـ endpoint السادس)
    /// </summary>
    public class AvailableTimeSlotResponse
    {
        /// <summary>
        /// الوقت بصيغة 24-hour (HH:mm)
        /// </summary>
        public string Time { get; set; } = string.Empty;

        /// <summary>
        /// هل الفترة متاحة للحجز
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// هل الفترة محجوزة
        /// </summary>
        public bool IsBooked { get; set; }

        /// <summary>
        /// هل الفترة فات وقتها
        /// </summary>
        public bool IsPast { get; set; }
    }
}
