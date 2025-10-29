using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Appointment
{
    public class AddExceptionalDateRequest
    {
        [Required(ErrorMessage = "التاريخ مطلوب")]
        public DateTime Date { get; set; }

        public string FromTime { get; set; } = string.Empty; // Format: "HH:mm"
        public string ToTime { get; set; } = string.Empty;   // Format: "HH:mm"
        public string FromPeriod { get; set; } = "AM";       // "AM" or "PM"
        public string ToPeriod { get; set; } = "PM";         // "AM" or "PM"

        [Required(ErrorMessage = "حالة الإغلاق مطلوبة")]
        public bool IsClosed { get; set; }
    }
}
