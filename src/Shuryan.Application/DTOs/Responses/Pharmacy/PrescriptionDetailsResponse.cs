namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    /// <summary>
    /// Detailed prescription response
    /// </summary>
    public class PrescriptionDetailsResponse
    {
        public Guid OrderId { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public PrescriptionPatientInfo Patient { get; set; } = null!;
        public PrescriptionDoctorInfo Doctor { get; set; } = null!;
        public List<PrescriptionMedicationItemResponse> Medications { get; set; } = new();
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Patient information in prescription
    /// </summary>
    public class PrescriptionPatientInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    /// <summary>
    /// Doctor information in prescription
    /// </summary>
    public class PrescriptionDoctorInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
    }
}
