using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Appointment
{
    public class CreateAppointmentRequest
    {
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }
        public ConsultationTypeEnum ConsultationType { get; set; } 
    }
}

