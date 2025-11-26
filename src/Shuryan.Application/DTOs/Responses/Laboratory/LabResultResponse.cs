using Shuryan.Application.DTOs.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    public class LabResultResponse : BaseAuditableDto
    {
        public Guid LabOrderId { get; set; }
        public Guid LabTestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string TestCode { get; set; } = string.Empty;
        public string ResultValue { get; set; } = string.Empty;
        public string? ReferenceRange { get; set; }
        public string? Unit { get; set; }
        public string? Notes { get; set; }
        public string? AttachmentUrl { get; set; }
    }
}
