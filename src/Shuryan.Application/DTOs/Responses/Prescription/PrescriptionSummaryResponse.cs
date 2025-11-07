using Shuryan.Core.Enums;
using System;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Lightweight prescription summary for lists and analytics
    /// </summary>
    public class PrescriptionSummaryResponse
    {
        public Guid Id { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int MedicationsCount { get; set; }
        public PrescriptionStatus Status { get; set; }
        public DateTime? DispensedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasPharmacyOrder { get; set; }
    }
}
