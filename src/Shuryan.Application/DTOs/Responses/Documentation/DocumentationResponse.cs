using System;

namespace Shuryan.Application.DTOs.Responses.Documentation
{
    /// <summary>
    /// Response لبيانات توثيق الكشف
    /// </summary>
    public class DocumentationResponse
    {
        public Guid ConsultationRecordId { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? HistoryOfPresentIllness { get; set; }
        public string? PhysicalExamination { get; set; }
        public string? Diagnosis { get; set; }
        public string? ManagementPlan { get; set; }
        public int SessionType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
