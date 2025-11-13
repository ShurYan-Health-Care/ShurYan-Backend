using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    /// <summary>
    /// Request DTO لتحديث ساعات وأيام عمل الصيدلية
    /// يدعم Partial Update - أرسل فقط الأيام التي تريد تحديثها
    /// </summary>
    public class UpdatePharmacyWorkingHoursRequest
    {
        /// <summary>
        /// جدول العمل الأسبوعي - أرسل فقط الأيام التي تريد تحديثها
        /// </summary>
        [Required(ErrorMessage = "جدول العمل الأسبوعي مطلوب")]
        public WeeklyScheduleDto WeeklySchedule { get; set; } = null!;
    }

    /// <summary>
    /// جدول العمل الأسبوعي للصيدلية
    /// </summary>
    public class WeeklyScheduleDto
    {
        public DayScheduleDto? Saturday { get; set; }
        public DayScheduleDto? Sunday { get; set; }
        public DayScheduleDto? Monday { get; set; }
        public DayScheduleDto? Tuesday { get; set; }
        public DayScheduleDto? Wednesday { get; set; }
        public DayScheduleDto? Thursday { get; set; }
        public DayScheduleDto? Friday { get; set; }
    }

    /// <summary>
    /// جدول عمل يوم واحد
    /// </summary>
    public class DayScheduleDto
    {
        /// <summary>
        /// هل اليوم ده شغال؟
        /// </summary>
        [Required(ErrorMessage = "حالة اليوم مطلوبة")]
        public bool Enabled { get; set; }

        /// <summary>
        /// الساعة (من) - مثال: "09" أو "02"
        /// </summary>
        [StringLength(2, MinimumLength = 1, ErrorMessage = "الساعة يجب أن تكون رقم من 1-2 خانة")]
        public string? FromTime { get; set; }

        /// <summary>
        /// الساعة (إلى) - مثال: "05" أو "11"
        /// </summary>
        [StringLength(2, MinimumLength = 1, ErrorMessage = "الساعة يجب أن تكون رقم من 1-2 خانة")]
        public string? ToTime { get; set; }

        /// <summary>
        /// الفترة (من) - "AM" أو "PM"
        /// </summary>
        [RegularExpression("^(AM|PM)$", ErrorMessage = "الفترة يجب أن تكون AM أو PM")]
        public string? FromPeriod { get; set; }

        /// <summary>
        /// الفترة (إلى) - "AM" أو "PM"
        /// </summary>
        [RegularExpression("^(AM|PM)$", ErrorMessage = "الفترة يجب أن تكون AM أو PM")]
        public string? ToPeriod { get; set; }
    }
}
