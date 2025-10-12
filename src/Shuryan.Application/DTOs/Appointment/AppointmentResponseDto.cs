using Shuryan.Application.DTOs.Base;
using Shuryan.Application.DTOs.Doctor;
using Shuryan.Application.DTOs.Patient;
using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Appointment
{
    public class AppointmentResponseDto : BaseAuditableDto
    {
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid? PreviousAppointmentId { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }
        public ConsultationTypeEnum ConsultationType { get; set; }
        public decimal ConsultationFee { get; set; }
        public int SessionDurationMinutes { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public PatientBasicDto? Patient { get; set; }
        public DoctorBasicDto? Doctor { get; set; }
        public ConsultationRecordDto? ConsultationRecord { get; set; }
    }

}
