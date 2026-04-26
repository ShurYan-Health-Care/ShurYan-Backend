using System;
using System.ComponentModel.DataAnnotations.Schema;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.Core.Entities.Medical
{
    public class EmergencyEvent : AuditableEntity
    {
        [ForeignKey("Patient")]
        public Guid PatientId { get; set; }

        [ForeignKey("ActivatingDoctor")]
        public Guid ActivatingDoctorId { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string MedicalRecordSnapshot { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public EmergencyEventStatus Status { get; set; } = EmergencyEventStatus.Dispatched;

        // Navigation Properties
        public virtual Patient Patient { get; set; } = null!;
        public virtual Doctor ActivatingDoctor { get; set; } = null!;
    }
}
