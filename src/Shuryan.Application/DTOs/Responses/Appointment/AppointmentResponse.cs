using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Appointment
{
    public class AppointmentResponse : BaseAuditableDto
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
        
        // 🆕 Session Times
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        
        // 🆕 Patient Info
        public string? PatientName { get; set; }
        public int? PatientAge { get; set; }
        public string? PatientProfileImageUrl { get; set; }
        
        // 🆕 Prescription ID
        public Guid? PrescriptionId { get; set; }
        
        // Navigation Properties
        public PatientBasicResponse? Patient { get; set; }
        public DoctorBasicResponse? Doctor { get; set; }
        public ConsultationRecordResponse? ConsultationRecord { get; set; }
    }
}

