using System;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Current active medication for a patient
    /// </summary>
    public class CurrentMedicationResponse
    {
        public Guid MedicationId { get; set; }
        public string BrandName { get; set; } = null!;
        public string? GenericName { get; set; }
        public string? Strength { get; set; }
        public string DosageForm { get; set; } = null!;
        
        // From prescription
        public Guid PrescriptionId { get; set; }
        public string PrescriptionNumber { get; set; } = null!;
        public DateTime PrescribedDate { get; set; }
        
        // Dosage info
        public string Dosage { get; set; } = null!;
        public string Frequency { get; set; } = null!;
        public int? DurationInDays { get; set; }
        public string? SpecialInstructions { get; set; }
        
        // Doctor info
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public string? DoctorSpecialization { get; set; }
        
        // Status
        public bool IsDispensed { get; set; }
        public DateTime? DispensedDate { get; set; }
        public int? RemainingDays { get; set; }
        
        // Additional date fields
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysRemaining { get; set; }
        public string? MedicationName { get; set; }
    }
}
