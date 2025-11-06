using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuryan.Core.Entities.External.Pharmacies
{
    public class DispensingRecord : AuditableEntity
    {
        [ForeignKey("Prescription")]
        public Guid PrescriptionId { get; set; }

        [ForeignKey("Pharmacy")]
        public Guid PharmacyId { get; set; }

        [ForeignKey("Patient")]
        public Guid PatientId { get; set; }

        public DateTime DispensedAt { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;

        public decimal TotalCost { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual Prescription Prescription { get; set; } = null!;
        public virtual Pharmacy Pharmacy { get; set; } = null!;
        public virtual Patient Patient { get; set; } = null!;
        public virtual ICollection<DispensedMedicationItem> DispensedMedications { get; set; } = new HashSet<DispensedMedicationItem>();
    }
}
