using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    /// <summary>
    /// السجل الطبي الكامل للمريض
    /// </summary>
    public class PatientMedicalRecordResponse
    {
        public Guid PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public DateTime? LastUpdatedAt { get; set; }
        
        public List<DrugAllergyResponse> DrugAllergies { get; set; } = new List<DrugAllergyResponse>();
        public List<CurrentMedicationResponse> CurrentMedications { get; set; } = new List<CurrentMedicationResponse>();
        public List<ChronicDiseaseResponse> ChronicDiseases { get; set; } = new List<ChronicDiseaseResponse>();
        public List<PreviousSurgeryResponse> PreviousSurgeries { get; set; } = new List<PreviousSurgeryResponse>();
    }

    /// <summary>
    /// الحساسية من الأدوية
    /// </summary>
    public class DrugAllergyResponse
    {
        public Guid Id { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public string Reaction { get; set; } = string.Empty; // أثر الدواء على المريض
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// الأدوية الحالية
    /// </summary>
    public class CurrentMedicationResponse
    {
        public Guid Id { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty; // الجرعة
        public string Frequency { get; set; } = string.Empty; // التكرار
        public DateTime? StartDate { get; set; } // تاريخ البدء
        public string Reason { get; set; } = string.Empty; // السبب
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// الأمراض المزمنة
    /// </summary>
    public class ChronicDiseaseResponse
    {
        public Guid Id { get; set; }
        public string DiseaseName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// العمليات الجراحية السابقة
    /// </summary>
    public class PreviousSurgeryResponse
    {
        public Guid Id { get; set; }
        public string SurgeryName { get; set; } = string.Empty; // اسم أو نوع العملية
        public DateTime? SurgeryDate { get; set; } // تاريخ العملية
        public DateTime CreatedAt { get; set; }
    }
}
