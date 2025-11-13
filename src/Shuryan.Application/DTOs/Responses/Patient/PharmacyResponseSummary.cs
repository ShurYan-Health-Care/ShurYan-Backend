using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Patient
{
    public class PharmacyResponseSummary
    {
        public Guid PharmacyId { get; set; }
        public string PharmacyName { get; set; } = string.Empty;
        public string PharmacyPhone { get; set; } = string.Empty;
        public string PharmacyAddress { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public bool DeliveryAvailable { get; set; }
        public string EstimatedDeliveryTime { get; set; } = string.Empty;
        public DateTime RespondedAt { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string PharmacyNotes { get; set; } = string.Empty;
        public List<PatientMedicationResponse> Medications { get; set; } = new();
        
        // إحصائيات سريعة
        public int AvailableMedicationsCount { get; set; }
        public int UnavailableMedicationsCount { get; set; }
        public int AlternativesOfferedCount { get; set; }
    }
}
