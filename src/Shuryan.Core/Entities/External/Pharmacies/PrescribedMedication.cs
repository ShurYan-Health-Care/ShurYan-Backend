using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;

namespace Shuryan.Core.Entities.External.Pharmacies
{
    public class PrescribedMedication
    {
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string? SpecialInstructions { get; set; }

        [ForeignKey("MedicationPrescription")]
        public Guid MedicationPrescriptionId { get; set; }

        [ForeignKey("Medication")]
        public Guid MedicationId { get; set; }

        // Navigation Properties
        public virtual Prescription MedicationPrescription { get; set; } = null!;
        public virtual Medication Medication { get; set; } = null!;
    }
}
