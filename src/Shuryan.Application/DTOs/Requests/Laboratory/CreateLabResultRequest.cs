using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
    public class CreateLabResultRequest
    {
        public Guid LabOrderId { get; set; }
        public Guid LabTestId { get; set; }
        public string ResultValue { get; set; } = string.Empty;
        public string? ReferenceRange { get; set; }
        public string? Unit { get; set; }
        public string? Notes { get; set; }
    }
}
