namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyOrderMedicationResponse
    {
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string? SpecialInstructions { get; set; }
    }
}
