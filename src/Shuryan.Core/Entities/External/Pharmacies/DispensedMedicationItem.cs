using Shuryan.Core.Entities.Base;
using System;
using System.Collections.Generic;

namespace Shuryan.Core.Entities.External.Pharmacies
{
    public class DispensedMedicationItem : AuditableEntity
    {
        public Guid DispensingRecordId { get; set; }
        public DispensingRecord DispensingRecord { get; set; } = null!;

        public Guid MedicationId { get; set; }
        public Medication Medication { get; set; } = null!;

        public int QuantityDispensed { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}