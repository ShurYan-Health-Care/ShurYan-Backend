using System;
using System.ComponentModel.DataAnnotations.Schema;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.Core.Entities.Medical
{
    public class VideoSession : AuditableEntity
    {
        [ForeignKey("Appointment")]
        public Guid AppointmentId { get; set; }

        [ForeignKey("Doctor")]
        public Guid DoctorId { get; set; }

        [ForeignKey("Patient")]
        public Guid PatientId { get; set; }

        public string AgoraChannelName { get; set; } = string.Empty;

        public VideoSessionStatus Status { get; set; } = VideoSessionStatus.Waiting;

        public DateTime? DoctorJoinedAt { get; set; }

        public DateTime? PatientJoinedAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public int? DurationSeconds { get; set; }

        public VideoSessionEndReason? EndReason { get; set; }

        public virtual Appointment Appointment { get; set; } = null!;
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual Patient Patient { get; set; } = null!;
    }
}
