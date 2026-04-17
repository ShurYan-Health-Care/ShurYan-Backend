using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.Core.Entities.Medical
{
    public class TelemedicineSession : AuditableEntity
    {
        [ForeignKey("Appointment")]
        public Guid AppointmentId { get; set; }

        [ForeignKey("Doctor")]
        public Guid DoctorId { get; set; }

        [ForeignKey("Patient")]
        public Guid PatientId { get; set; }

        [Required, MaxLength(100)]
        public string RoomId { get; set; } = string.Empty;

        public TelemedicineSessionStatus Status { get; set; } = TelemedicineSessionStatus.Waiting;

        [MaxLength(256)]
        public string? DoctorConnectionId { get; set; }

        [MaxLength(256)]
        public string? PatientConnectionId { get; set; }

        public DateTime? DoctorJoinedAt { get; set; }
        public DateTime? PatientJoinedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? DurationSeconds { get; set; }
        public SessionEndReason? EndReason { get; set; }

        // Navigation Properties
        public virtual Appointment Appointment { get; set; } = null!;
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual Patient Patient { get; set; } = null!;
    }
}
