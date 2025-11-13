using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyOrderPrescriptionResponse
    {
        public List<PharmacyOrderMedicationResponse> Medications { get; set; } = new();
        public string? DoctorNotes { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
