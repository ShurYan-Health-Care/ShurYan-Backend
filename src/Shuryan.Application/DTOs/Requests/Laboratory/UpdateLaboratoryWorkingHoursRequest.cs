using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
 
        public class UpdateLaboratoryWorkingHoursRequest
        {
                [Required(ErrorMessage = "جدول العمل الأسبوعي مطلوب")]
                public LaboratoryWeeklyScheduleDto WeeklySchedule { get; set; } = null!;
        }

        public class LaboratoryWeeklyScheduleDto
        {
                public LaboratoryDayScheduleDto? Saturday { get; set; }
                public LaboratoryDayScheduleDto? Sunday { get; set; }
                public LaboratoryDayScheduleDto? Monday { get; set; }
                public LaboratoryDayScheduleDto? Tuesday { get; set; }
                public LaboratoryDayScheduleDto? Wednesday { get; set; }
                public LaboratoryDayScheduleDto? Thursday { get; set; }
                public LaboratoryDayScheduleDto? Friday { get; set; }
        }

        public class LaboratoryDayScheduleDto
        {
                [Required(ErrorMessage = "حالة اليوم مطلوبة")]
                public bool Enabled { get; set; }
                [StringLength(2, MinimumLength = 1, ErrorMessage = "الساعة يجب أن تكون رقم من 1-2 خانة")]
                public string? FromTime { get; set; }

                [StringLength(2, MinimumLength = 1, ErrorMessage = "الساعة يجب أن تكون رقم من 1-2 خانة")]
                public string? ToTime { get; set; }

                [RegularExpression("^(AM|PM)$", ErrorMessage = "الفترة يجب أن تكون AM أو PM")]
                public string? FromPeriod { get; set; }

                [RegularExpression("^(AM|PM)$", ErrorMessage = "الفترة يجب أن تكون AM أو PM")]
                public string? ToPeriod { get; set; }
        }
}
