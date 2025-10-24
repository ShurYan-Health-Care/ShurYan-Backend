using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Result of dispensing a prescription
    /// </summary>
    public class DispenseResult
    {
        public Guid PrescriptionId { get; set; }
        public Guid DispensingRecordId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        
        public DateTime DispensedAt { get; set; }
        public Guid PharmacyId { get; set; }
        public string PharmacyName { get; set; } = null!;
        public Guid PharmacistId { get; set; }
        public string PharmacistName { get; set; } = null!;

        public decimal TotalCost { get; set; }
        public int TotalItemsDispensed { get; set; }
        
        public List<DispensedItem> DispensedItems { get; set; } = new();
        
        // Receipt/Invoice number
        public string ReceiptNumber { get; set; } = null!;
    }

    public class DispensedItem
    {
        public Guid MedicationId { get; set; }
        public string MedicationName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
