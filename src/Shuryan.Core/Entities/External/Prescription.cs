using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Entities.External
{/// <summary>
 /// بيمثل روشتة الأدوية اللي بيكتبها الدكتور
 /// </summary>
    public class Prescription
    {
        public Guid Id { get; set; }
        public string PrescriptionNumber { get; set; }
        public string DigitalSignature { get; set; }
        public string? GeneralInstructions { get; set; }
        public DateTime IssuedDate { get; set; }
        public string? FollowUpInstructions { get; set; }
        public bool IsDigitallyShared { get; set; } = false;
        public DateTime? SharedAt { get; set; }
        // --- العلاقات ---
        [ForeignKey("Appointment")]
        public Guid AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; } = null!;

        [ForeignKey("Doctor")]
        public Guid DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; } = null!;

        [ForeignKey("Patient")]
        public Guid PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;

        [ForeignKey("PharmacyOrder")]
        public Guid PharmacyOrderId { get; set; }
        public virtual PharmacyOrder PharmacyOrder { get; set; }
        public virtual ICollection<PrescribedMedication> PrescribedMedications { get; set; } = new HashSet<PrescribedMedication>();

    }
}
