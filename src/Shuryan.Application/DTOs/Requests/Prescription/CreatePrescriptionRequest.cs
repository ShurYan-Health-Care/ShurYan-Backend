using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Prescription
{
    public class CreatePrescriptionRequest
    {
        [Required(ErrorMessage = "Appointment ID is required")]
        public Guid AppointmentId { get; set; }

        [Required(ErrorMessage = "Doctor ID is required")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "Patient ID is required")]
        public Guid PatientId { get; set; }

        [Required(ErrorMessage = "Prescription number is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Prescription number must be between 3-100 characters")]
        public string PrescriptionNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Digital signature is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Digital signature must be between 10-1000 characters")]
        public string DigitalSignature { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "General instructions cannot exceed 1000 characters")]
        public string? GeneralInstructions { get; set; }

        [Required(ErrorMessage = "At least one prescribed medication is required")]
        [MinLength(1, ErrorMessage = "At least one prescribed medication is required")]
        public IEnumerable<CreatePrescribedMedicationRequest> PrescribedMedications { get; set; } = new List<CreatePrescribedMedicationRequest>();
    }
}

