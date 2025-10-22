using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Patient's medication history
    /// </summary>
    public class MedicationHistoryResponse
    {
        public Guid MedicationId { get; set; }
        public string BrandName { get; set; } = null!;
        public string? GenericName { get; set; }
        public string? Strength { get; set; }
        public string DosageForm { get; set; } = null!;
        
        // Usage statistics
        public int TotalPrescriptions { get; set; }
        public DateTime FirstPrescribed { get; set; }
        public DateTime LastPrescribed { get; set; }
        public int TotalDaysUsed { get; set; }
        
        // Prescribing doctors
        public List<PrescribingDoctorInfo> PrescribedBy { get; set; } = new();
        
        // All prescriptions for this medication
        public List<MedicationPrescriptionSummary> Prescriptions { get; set; } = new();
        
        // Adverse reactions or notes (if any)
        public List<string> Notes { get; set; } = new();
    }

    public class PrescribingDoctorInfo
    {
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public string? Specialization { get; set; }
        public int PrescriptionCount { get; set; }
    }

    public class MedicationPrescriptionSummary
    {
        public Guid PrescriptionId { get; set; }
        public string PrescriptionNumber { get; set; } = null!;
        public DateTime PrescribedDate { get; set; }
        public string Dosage { get; set; } = null!;
        public string Frequency { get; set; } = null!;
        public int? DurationInDays { get; set; }
        public bool WasDispensed { get; set; }
        public string Status { get; set; } = null!;
    }
}
