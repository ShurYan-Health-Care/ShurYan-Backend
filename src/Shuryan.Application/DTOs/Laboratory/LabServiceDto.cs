using Shuryan.Application.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class LabServiceDto : BaseAuditableDto
    {
        public Guid LaboratoryId { get; set; }
        public Guid LabTestId { get; set; }
        public string LabTestName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string? LabSpecificNotes { get; set; }
    }
}
