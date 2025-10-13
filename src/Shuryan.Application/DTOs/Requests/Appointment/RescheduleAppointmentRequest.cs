using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Appointment
{
    public class RescheduleAppointmentRequest
    {
        public DateTime NewScheduledStartTime { get; set; }
        public DateTime NewScheduledEndTime { get; set; }
    }
}

