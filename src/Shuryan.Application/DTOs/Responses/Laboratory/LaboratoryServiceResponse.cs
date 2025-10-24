using System;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    /// <summary>
    /// Simplified lab service response - without audit fields
    /// </summary>
    public class LaboratoryServiceResponse
    {
        public Guid LabTestId { get; set; }
        public string LabTestName { get; set; } = string.Empty;
        public string LabTestCategory { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string? LabSpecificNotes { get; set; }
    }
}
