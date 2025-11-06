using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Entities.External.Pharmacies
{
	public class Prescription : AuditableEntity
	{
        public string PrescriptionNumber { get; set; } = null!;
        public string DigitalSignature { get; set; } = null!;
        public string? GeneralInstructions { get; set; }
        
        public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Active;
        public DateTime? DispensedAt { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        
        [ForeignKey("Appointment")]
        public Guid? AppointmentId { get; set; }
        public virtual Appointment? Appointment { get; set; }

        [ForeignKey("Doctor")]
        public Guid DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; } = null!;

        [ForeignKey("Patient")]
        public Guid PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;

        public virtual PharmacyOrder? PharmacyOrder { get; set; }
        public virtual ICollection<PrescribedMedication> PrescribedMedications { get; set; } = new HashSet<PrescribedMedication>();

    }
}
