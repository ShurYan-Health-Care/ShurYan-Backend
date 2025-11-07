using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    public class PrescriptionResponse : BaseAuditableDto
    {
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string DigitalSignature { get; set; } = string.Empty;
        public Guid? AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public string? GeneralInstructions { get; set; }
        
        public PrescriptionStatus Status { get; set; }
        public DateTime? DispensedAt { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        
        public DoctorBasicResponse? Doctor { get; set; }
        public PatientBasicResponse? Patient { get; set; }
        public IEnumerable<PrescribedMedicationResponse> PrescribedMedications { get; set; } = new List<PrescribedMedicationResponse>();
    }
}

