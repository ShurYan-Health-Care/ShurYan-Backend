namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    /// <summary>
    /// Response DTO لساعات وأيام عمل الصيدلية
    /// </summary>
    public class PharmacyWorkingHoursResponse
    {
        public DayScheduleResponse Saturday { get; set; } = new();
        public DayScheduleResponse Sunday { get; set; } = new();
        public DayScheduleResponse Monday { get; set; } = new();
        public DayScheduleResponse Tuesday { get; set; } = new();
        public DayScheduleResponse Wednesday { get; set; } = new();
        public DayScheduleResponse Thursday { get; set; } = new();
        public DayScheduleResponse Friday { get; set; } = new();
    }

    /// <summary>
    /// جدول عمل يوم واحد
    /// </summary>
    public class DayScheduleResponse
    {
        /// <summary>
        /// هل اليوم ده شغال؟
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// الساعة (من) - مثال: "09" أو "02"
        /// </summary>
        public string FromTime { get; set; } = string.Empty;

        /// <summary>
        /// الساعة (إلى) - مثال: "05" أو "11"
        /// </summary>
        public string ToTime { get; set; } = string.Empty;

        /// <summary>
        /// الفترة (من) - "AM" أو "PM"
        /// </summary>
        public string FromPeriod { get; set; } = string.Empty;

        /// <summary>
        /// الفترة (إلى) - "AM" أو "PM"
        /// </summary>
        public string ToPeriod { get; set; } = string.Empty;
    }
}
