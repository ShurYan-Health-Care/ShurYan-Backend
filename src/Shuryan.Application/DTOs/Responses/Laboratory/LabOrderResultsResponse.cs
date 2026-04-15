using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    public class LabOrderResultsResponse
    {
        public Guid OrderId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string LaboratoryName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public List<LabResultDetailDto> Results { get; set; } = new();
    }

    public class LabResultDetailDto
    {
        public Guid LabResultId { get; set; }
        public Guid TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string TestCode { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ResultValue { get; set; } = string.Empty;
        public string? ReferenceRange { get; set; }
        public string? Unit { get; set; }
        public string? Notes { get; set; }
        public string? AttachmentUrl { get; set; }
        public bool IsAbnormal { get; set; }
    }
}
