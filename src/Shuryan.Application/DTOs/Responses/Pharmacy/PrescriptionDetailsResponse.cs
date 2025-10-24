using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Responses.Prescription;

namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PrescriptionDetailsResponse : BaseAuditableDto
    {
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string DigitalSignature { get; set; } = string.Empty;
        public string? GeneralInstructions { get; set; }
        public string? FollowUpInstructions { get; set; }
        public bool IsDigitallyShared { get; set; }
        public DateTime? SharedAt { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public IEnumerable<PrescribedMedicationResponse> Medications { get; set; } = new List<PrescribedMedicationResponse>();
    }
}
