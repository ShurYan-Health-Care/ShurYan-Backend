using System;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Response لعنصر في قائمة الروشتات (ملخص)
    /// </summary>
    public class PrescriptionListItemResponse
    {
        public Guid Id { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
