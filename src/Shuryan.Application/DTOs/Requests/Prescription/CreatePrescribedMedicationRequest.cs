using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Prescription
{
    public class CreatePrescribedMedicationRequest
    {
        [Required(ErrorMessage = "Medication ID is required")]
        public Guid MedicationId { get; set; }

        [Required(ErrorMessage = "Dosage is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Dosage must be between 2-200 characters")]
        public string Dosage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Frequency is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Frequency must be between 2-200 characters")]
        public string Frequency { get; set; } = string.Empty;

        [Required(ErrorMessage = "Duration days is required")]
        [Range(1, 365, ErrorMessage = "Duration must be between 1-365 days")]
        public int DurationDays { get; set; }

        [StringLength(500, ErrorMessage = "Special instructions cannot exceed 500 characters")]
        public string? SpecialInstructions { get; set; }
    }
}
