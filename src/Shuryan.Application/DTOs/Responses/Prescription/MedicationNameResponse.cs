using Shuryan.Core.Enums.Pharmacy;
using System;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Response بسيط لأسماء الأدوية - للاستخدام في Dropdowns
    /// </summary>
    public class MedicationNameResponse
    {
        public Guid Id { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? Strength { get; set; }
        public DosageForm DosageForm { get; set; }

    }
}
