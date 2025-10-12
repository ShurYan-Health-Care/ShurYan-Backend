using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    public class PrescribedMedicationResponse
    {
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string? SpecialInstructions { get; set; }
        public Guid MedicationId { get; set; }
        public MedicationResponse? Medication { get; set; }
    }
}
