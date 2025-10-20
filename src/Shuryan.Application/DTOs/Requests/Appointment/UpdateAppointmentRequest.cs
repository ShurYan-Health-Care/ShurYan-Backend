using Shuryan.Core.Enums.Appointments;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Appointment
{
    public class UpdateAppointmentRequest
    {
        public DateTime? ScheduledStartTime { get; set; }

        public DateTime? ScheduledEndTime { get; set; }

        public ConsultationTypeEnum? ConsultationType { get; set; }

        public AppointmentStatus? Status { get; set; }

        [StringLength(500, ErrorMessage = "Cancellation reason cannot exceed 500 characters")]
        public string? CancellationReason { get; set; }
    }
}
