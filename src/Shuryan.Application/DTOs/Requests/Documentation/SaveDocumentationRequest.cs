using System;

namespace Shuryan.Application.DTOs.Requests.Documentation
{
    /// <summary>
    /// Request لحفظ/تحديث توثيق الكشف
    /// جميع الحقول optional لدعم الحفظ الجزئي
    /// </summary>
    public class SaveDocumentationRequest
    {
        public string? ChiefComplaint { get; set; }
        public string? HistoryOfPresentIllness { get; set; }
        public string? PhysicalExamination { get; set; }
        public string? Diagnosis { get; set; }
        public string? ManagementPlan { get; set; }
        public int SessionType { get; set; } = 1; // Default: GeneralConsultation
    }
}
