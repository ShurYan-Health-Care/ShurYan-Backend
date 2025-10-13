using Shuryan.Application.DTOs.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    public class LabServiceResponse : BaseAuditableDto
    {
        public Guid LaboratoryId { get; set; }
        public Guid LabTestId { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string? LabSpecificNotes { get; set; }
    }
}
