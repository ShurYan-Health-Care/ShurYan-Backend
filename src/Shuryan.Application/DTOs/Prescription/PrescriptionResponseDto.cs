using Shuryan.Application.DTOs.Base;
using Shuryan.Application.DTOs.Doctor;
using Shuryan.Application.DTOs.Patient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Prescription
{
    public class PrescriptionResponseDto : BaseAuditableDto
    {
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string DigitalSignature { get; set; } = string.Empty;
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public string? GeneralInstructions { get; set; }
        public string? FollowUpInstructions { get; set; }
        public bool IsDigitallyShared { get; set; }
        public DateTime? SharedAt { get; set; }
        public DoctorBasicDto? Doctor { get; set; }
        public PatientBasicDto? Patient { get; set; }
        public IEnumerable<PrescribedMedicationDto> PrescribedMedications { get; set; } = new List<PrescribedMedicationDto>();
    }
}
