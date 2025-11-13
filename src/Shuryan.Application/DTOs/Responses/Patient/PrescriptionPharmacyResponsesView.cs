using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Patient
{
    public class PrescriptionPharmacyResponsesView
    {
        public Guid PrescriptionId { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public int TotalMedications { get; set; }
        public int TotalPharmacyResponses { get; set; }
        public List<PharmacyResponseSummary> PharmacyResponses { get; set; } = new();
    }
}
