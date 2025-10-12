using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Prescription
{
    public class CreatePrescriptionRequest
    {
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string DigitalSignature { get; set; } = string.Empty;
        public string? GeneralInstructions { get; set; }
        public string? FollowUpInstructions { get; set; }
        public IEnumerable<CreatePrescribedMedicationRequest> PrescribedMedications { get; set; } = new List<CreatePrescribedMedicationRequest>();
    }
}

