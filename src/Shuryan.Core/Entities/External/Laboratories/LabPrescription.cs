using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical;

namespace Shuryan.Core.Entities.External.Laboratories
{
    public class LabPrescription : AuditableEntity
    {
        [ForeignKey("Appointment")]
        public Guid AppointmentId { get; set; }

        [ForeignKey("Doctor")]
        public Guid DoctorId { get; set; }

        [ForeignKey("Patient")]
        public Guid PatientId { get; set; }

        public string? GeneralNotes { get; set; }

        // Navigation Properties
        public virtual Appointment Appointment { get; set; } = null!;
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual Patient Patient { get; set; } = null!;
        public virtual ICollection<LabPrescriptionItem> Items { get; set; } = new HashSet<LabPrescriptionItem>();
        public virtual LabOrder? LabOrder { get; set; }
    }
}
