using System;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    public class LabOrderTestResponse
    {
        public Guid LabTestId { get; set; }
        public string LabTestName { get; set; } = string.Empty;
        public string LabTestCategory { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
