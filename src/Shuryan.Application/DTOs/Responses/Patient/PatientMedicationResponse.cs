namespace Shuryan.Application.DTOs.Responses.Patient
{
    public class PatientMedicationResponse
    {
        public string MedicationName { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public PatientAlternativeMedicationResponse? AlternativeOne { get; set; }
    }
}
