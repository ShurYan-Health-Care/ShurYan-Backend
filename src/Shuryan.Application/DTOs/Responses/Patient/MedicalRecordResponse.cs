using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Patient
{
    /// <summary>
    /// Complete medical record response for patient profile
    /// </summary>
    public class MedicalRecordResponse
    {
        public Guid PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public DateTime? LastUpdatedAt { get; set; }
        public List<DrugAllergyItem> DrugAllergies { get; set; } = new List<DrugAllergyItem>();
        public List<ChronicDiseaseItem> ChronicDiseases { get; set; } = new List<ChronicDiseaseItem>();
        public List<CurrentMedicationItem> CurrentMedications { get; set; } = new List<CurrentMedicationItem>();
        public List<PreviousSurgeryItem> PreviousSurgeries { get; set; } = new List<PreviousSurgeryItem>();
    }

    public class DrugAllergyItem
    {
        public Guid Id { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public string? Reaction { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ChronicDiseaseItem
    {
        public Guid Id { get; set; }
        public string DiseaseName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CurrentMedicationItem
    {
        public Guid Id { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public DateTime? StartDate { get; set; }
        public string? Reason { get; set; }
    }

    public class PreviousSurgeryItem
    {
        public Guid Id { get; set; }
        public string SurgeryName { get; set; } = string.Empty;
        public DateTime? SurgeryDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
