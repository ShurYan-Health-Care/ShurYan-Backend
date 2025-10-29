namespace Shuryan.Application.DTOs.Responses.Appointment
{
    public class WeeklyScheduleResponse
    {
        public WeeklyScheduleDataDto WeeklySchedule { get; set; } = new WeeklyScheduleDataDto();
    }

    public class WeeklyScheduleDataDto
    {
        public DayScheduleResponseDto Saturday { get; set; } = new DayScheduleResponseDto();
        public DayScheduleResponseDto Sunday { get; set; } = new DayScheduleResponseDto();
        public DayScheduleResponseDto Monday { get; set; } = new DayScheduleResponseDto();
        public DayScheduleResponseDto Tuesday { get; set; } = new DayScheduleResponseDto();
        public DayScheduleResponseDto Wednesday { get; set; } = new DayScheduleResponseDto();
        public DayScheduleResponseDto Thursday { get; set; } = new DayScheduleResponseDto();
        public DayScheduleResponseDto Friday { get; set; } = new DayScheduleResponseDto();
    }

    public class DayScheduleResponseDto
    {
        public bool Enabled { get; set; }
        public string FromTime { get; set; } = string.Empty;
        public string ToTime { get; set; } = string.Empty;
        public string FromPeriod { get; set; } = "AM";
        public string ToPeriod { get; set; } = "PM";
    }
}
