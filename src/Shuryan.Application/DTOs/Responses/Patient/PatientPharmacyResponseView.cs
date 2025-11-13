using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Patient
{
    public class PatientPharmacyResponseView
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string PharmacyName { get; set; } = string.Empty;
        public string PharmacyPhone { get; set; } = string.Empty;
        public string PrescriptionNumber { get; set; } = string.Empty;
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public List<PatientMedicationResponse> Medications { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public bool DeliveryAvailable { get; set; }
        public decimal DeliveryFee { get; set; }
        public string? PharmacyNotes { get; set; }
        public DateTime RespondedAt { get; set; }
        public DateTime SentAt { get; set; }
    }
}
