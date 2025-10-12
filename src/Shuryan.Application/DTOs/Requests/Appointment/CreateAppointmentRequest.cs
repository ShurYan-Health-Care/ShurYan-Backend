using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Appointment
{
    public class CreateAppointmentRequest
    {
        [Required(ErrorMessage = "Patient ID is required")]
        public Guid PatientId { get; set; }

        [Required(ErrorMessage = "Doctor ID is required")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "Scheduled start time is required")]
        public DateTime ScheduledStartTime { get; set; }

        [Required(ErrorMessage = "Scheduled end time is required")]
        public DateTime ScheduledEndTime { get; set; }

        [Required(ErrorMessage = "Consultation type is required")]
        public ConsultationTypeEnum ConsultationType { get; set; }

        // Custom validation: EndTime > StartTime should be done in FluentValidator
    }
}

