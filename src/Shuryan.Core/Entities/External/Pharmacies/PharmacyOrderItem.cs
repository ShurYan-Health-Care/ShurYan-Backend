using Shuryan.Core.Entities.Base;
using Shuryan.Core.Enums.Pharmacy;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuryan.Core.Entities.External.Pharmacies
{
    public class PharmacyOrderItem : AuditableEntity
    {
        [ForeignKey("PharmacyOrder")]
        public Guid PharmacyOrderId { get; set; }

        [ForeignKey("RequestedMedication")]
        public Guid RequestedMedicationId { get; set; }

        public PharmacyItemStatus Status { get; set; }

        // Available
        public int? AvailableQuantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }

        // Alternative
        [ForeignKey("AlternativeMedication")]
        public Guid? AlternativeMedicationId { get; set; }
        public decimal? AlternativeUnitPrice { get; set; }
        public string? AlternativeNotes { get; set; }

        // Navigation Properties
        public virtual PharmacyOrder PharmacyOrder { get; set; } = null!;
        public virtual Medication RequestedMedication { get; set; } = null!;
        public virtual Medication? AlternativeMedication { get; set; }
    }
}
