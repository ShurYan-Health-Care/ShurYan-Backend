using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Prescription
{
    public class CreatePrescriptionDto
    {
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string DigitalSignature { get; set; } = string.Empty;
        public string? GeneralInstructions { get; set; }
        public string? FollowUpInstructions { get; set; }
        public IEnumerable<CreatePrescribedMedicationDto> PrescribedMedications { get; set; } = new List<CreatePrescribedMedicationDto>();
    }

}
