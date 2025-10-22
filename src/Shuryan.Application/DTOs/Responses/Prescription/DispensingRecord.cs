using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Record of a prescription dispensing event
    /// </summary>
    public class DispensingRecord
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        
        public Guid PharmacyId { get; set; }
        public string PharmacyName { get; set; } = null!;
        public Guid PharmacistId { get; set; }
        public string PharmacistName { get; set; } = null!;
        
        public DateTime DispensedAt { get; set; }
        public decimal TotalCost { get; set; }
        public string? PaymentMethod { get; set; }
        public string ReceiptNumber { get; set; } = null!;
        
        public List<DispensedMedicationDetail> Medications { get; set; } = new();
        
        public string? PharmacistNotes { get; set; }
        public bool PatientSignatureConfirmed { get; set; }
    }

    public class DispensedMedicationDetail
    {
        public Guid MedicationId { get; set; }
        public string MedicationName { get; set; } = null!;
        public int QuantityDispensed { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
