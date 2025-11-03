using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Patient
{
    /// <summary>
    /// Request to update medical record
    /// Supports partial updates - only send the sections you want to update
    /// </summary>
    public class UpdateMedicalRecordRequest
    {
        public List<DrugAllergyItemRequest>? DrugAllergies { get; set; }
        public List<ChronicDiseaseItemRequest>? ChronicDiseases { get; set; }
        public List<CurrentMedicationItemRequest>? CurrentMedications { get; set; }
        public List<PreviousSurgeryItemRequest>? PreviousSurgeries { get; set; }
    }

    public class DrugAllergyItemRequest
    {
        /// <summary>
        /// If provided, updates existing item. If null, creates new item.
        /// </summary>
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Drug name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Drug name must be between 2-100 characters")]
        public string DrugName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Reaction cannot exceed 500 characters")]
        public string? Reaction { get; set; }
    }

    public class ChronicDiseaseItemRequest
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Disease name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Disease name must be between 2-200 characters")]
        public string DiseaseName { get; set; } = string.Empty;
    }

    public class CurrentMedicationItemRequest
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Medication name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Medication name must be between 2-200 characters")]
        public string MedicationName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Dosage cannot exceed 100 characters")]
        public string? Dosage { get; set; }

        [StringLength(100, ErrorMessage = "Frequency cannot exceed 100 characters")]
        public string? Frequency { get; set; }

        public DateTime? StartDate { get; set; }

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }
    }

    public class PreviousSurgeryItemRequest
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Surgery name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Surgery name must be between 2-200 characters")]
        public string SurgeryName { get; set; } = string.Empty;

        public DateTime? SurgeryDate { get; set; }
    }
}
