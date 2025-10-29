using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Appointment
{
    public class UpdateWeeklyScheduleRequest
    {
        [Required(ErrorMessage = "الجدول الأسبوعي مطلوب")]
        public WeeklyScheduleDto WeeklySchedule { get; set; } = new WeeklyScheduleDto();
    }

    public class WeeklyScheduleDto
    {
        public DayScheduleDto Saturday { get; set; } = new DayScheduleDto();
        public DayScheduleDto Sunday { get; set; } = new DayScheduleDto();
        public DayScheduleDto Monday { get; set; } = new DayScheduleDto();
        public DayScheduleDto Tuesday { get; set; } = new DayScheduleDto();
        public DayScheduleDto Wednesday { get; set; } = new DayScheduleDto();
        public DayScheduleDto Thursday { get; set; } = new DayScheduleDto();
        public DayScheduleDto Friday { get; set; } = new DayScheduleDto();
    }

    public class DayScheduleDto
    {
        public bool Enabled { get; set; }
        public string FromTime { get; set; } = string.Empty; // Format: "HH:mm"
        public string ToTime { get; set; } = string.Empty;   // Format: "HH:mm"
        public string FromPeriod { get; set; } = "AM";       // "AM" or "PM"
        public string ToPeriod { get; set; } = "PM";         // "AM" or "PM"
    }
}
