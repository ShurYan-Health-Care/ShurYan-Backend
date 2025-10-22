using Shuryan.Core.Entities.Base;
using System;
using System.Collections.Generic;

namespace Shuryan.Core.Entities.External.Pharmacies
{
    public class DispensingRecord : AuditableEntity
    {
        public Guid PrescriptionId { get; set; }
        public Prescription Prescription { get; set; } = null!;

        public Guid PharmacyId { get; set; }
        public Guid PharmacistId { get; set; }

        public DateTime DispensedAt { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;

        public decimal TotalCost { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PharmacistNotes { get; set; }

        public bool PatientSignatureConfirmed { get; set; }

        public ICollection<DispensedMedicationItem> DispensedMedications { get; set; } = new List<DispensedMedicationItem>();
    }

    public class DispensedMedicationItem : AuditableEntity
    {
        public Guid DispensingRecordId { get; set; }
        public DispensingRecord DispensingRecord { get; set; } = null!;

        public Guid MedicationId { get; set; }
        public Medication Medication { get; set; } = null!;

        public int QuantityDispensed { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
