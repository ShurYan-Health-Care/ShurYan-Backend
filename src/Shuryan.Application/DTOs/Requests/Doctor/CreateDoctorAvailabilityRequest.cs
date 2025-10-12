using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class CreateDoctorAvailabilityRequest
    {
        [Required(ErrorMessage = "Day of week is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Day of week must be valid")]
        public string DayOfWeek { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start time is required")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public TimeSpan EndTime { get; set; }
    }
}

