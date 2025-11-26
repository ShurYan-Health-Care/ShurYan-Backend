namespace Shuryan.Application.DTOs.Responses.Laboratory
{
        public class LaboratoryWorkingHoursResponse
        {
                public LaboratoryDayScheduleResponse Saturday { get; set; } = new();
                public LaboratoryDayScheduleResponse Sunday { get; set; } = new();
                public LaboratoryDayScheduleResponse Monday { get; set; } = new();
                public LaboratoryDayScheduleResponse Tuesday { get; set; } = new();
                public LaboratoryDayScheduleResponse Wednesday { get; set; } = new();
                public LaboratoryDayScheduleResponse Thursday { get; set; } = new();
                public LaboratoryDayScheduleResponse Friday { get; set; } = new();
        }

        public class LaboratoryDayScheduleResponse
        {
           
                public bool Enabled { get; set; }
                public string FromTime { get; set; } = string.Empty;
                public string ToTime { get; set; } = string.Empty;
                public string FromPeriod { get; set; } = string.Empty;
                public string ToPeriod { get; set; } = string.Empty;
        }
}
