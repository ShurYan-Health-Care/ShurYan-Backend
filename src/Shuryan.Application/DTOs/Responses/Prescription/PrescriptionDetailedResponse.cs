using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Response لروشتة معينة بتفاصيل الأدوية الكاملة
    /// </summary>
    public class PrescriptionDetailedResponse
    {
        public Guid Id { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public IEnumerable<MedicationDetailResponse> Medications { get; set; } = new List<MedicationDetailResponse>();
    }

    /// <summary>
    /// تفاصيل الدواء في الروشتة
    /// </summary>
    public class MedicationDetailResponse
    {
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string? SpecialInstructions { get; set; }
    }
}
