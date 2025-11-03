namespace Shuryan.Application.DTOs.Responses.Appointment
{
    /// <summary>
    /// Response DTO لجدول يوم واحد في الأسبوع
    /// حسب متطلبات الفرونت: dayOfWeek (0-6), isEnabled, fromTime/toTime (24-hour format)
    /// </summary>
    public class DayScheduleSlotResponse
    {
        /// <summary>
        /// رقم اليوم في الأسبوع: 0=الأحد, 1=الإثنين, ..., 6=السبت
        /// </summary>
        public int DayOfWeek { get; set; }

        /// <summary>
        /// هل اليوم شغال
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// وقت البداية بصيغة 24-hour (HH:mm) مثل "09:00"
        /// </summary>
        public string? FromTime { get; set; }

        /// <summary>
        /// وقت النهاية بصيغة 24-hour (HH:mm) مثل "17:00"
        /// </summary>
        public string? ToTime { get; set; }
    }
}
