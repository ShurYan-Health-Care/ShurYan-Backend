namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class OrderMedicationDetailResponse
    {
        public string MedicationName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
    }
}
