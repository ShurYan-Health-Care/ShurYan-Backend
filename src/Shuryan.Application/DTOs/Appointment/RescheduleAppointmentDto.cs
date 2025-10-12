using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Appointment
{
    public class RescheduleAppointmentDto
    {
        public DateTime NewScheduledStartTime { get; set; }
        public DateTime NewScheduledEndTime { get; set; }
    }

}
