namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    /// <summary>
    /// Response DTO for pending prescription in list
    /// </summary>
    public class PendingPrescriptionResponse
    {
        public Guid OrderId { get; set; }
        public Guid? PrescriptionId { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public int Status { get; set; }
        public DateTime ReceivedAt { get; set; }
    }

    /// <summary>
    /// Medication item in prescription
    /// </summary>
    public class PrescriptionMedicationItemResponse
    {
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string? SpecialInstructions { get; set; }
    }

    /// <summary>
    /// Paginated response for pending prescriptions
    /// </summary>
    public class PendingPrescriptionsListResponse
    {
        public List<PendingPrescriptionResponse> Prescriptions { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
