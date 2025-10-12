using Shuryan.Application.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class LabPrescriptionItemDto : BaseAuditableDto
    {
        public Guid LabTestId { get; set; }
        public string LabTestName { get; set; } = string.Empty;
        public string? DoctorNotes { get; set; }
    }

}
