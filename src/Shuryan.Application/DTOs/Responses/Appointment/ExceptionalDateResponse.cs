using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Appointment
{
    public class ExceptionalDatesListResponse
    {
        public List<ExceptionalDateResponse> ExceptionalDates { get; set; } = new List<ExceptionalDateResponse>();
    }

    public class ExceptionalDateResponse
    {
        public Guid Id { get; set; }
        public string Date { get; set; } = string.Empty; // Format: "YYYY-MM-DD"
        public string FromTime { get; set; } = string.Empty;
        public string ToTime { get; set; } = string.Empty;
        public string FromPeriod { get; set; } = "AM";
        public string ToPeriod { get; set; } = "PM";
        public bool IsClosed { get; set; }
    }
}
