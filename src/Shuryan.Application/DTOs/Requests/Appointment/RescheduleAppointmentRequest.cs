using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Appointment
{
    public class RescheduleAppointmentRequest
    {
        [Required(ErrorMessage = "New scheduled start time is required")]
        public DateTime NewScheduledStartTime { get; set; }

        [Required(ErrorMessage = "New scheduled end time is required")]
        public DateTime NewScheduledEndTime { get; set; }
    }
}

