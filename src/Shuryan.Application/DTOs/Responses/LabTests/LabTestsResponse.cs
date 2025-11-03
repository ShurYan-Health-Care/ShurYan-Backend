using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.LabTests
{
    /// <summary>
    /// Response لبيانات طلب التحاليل
    /// </summary>
    public class LabTestsResponse
    {
        public Guid LabRequestId { get; set; }
        public Guid AppointmentId { get; set; }
        public List<string> Tests { get; set; } = new List<string>();
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
