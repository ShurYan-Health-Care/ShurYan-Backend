using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Enums.Laboratory;

namespace Shuryan.Core.Entities.External.Laboratories
{
    public class LabTest : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public LabTestCategory Category { get; set; }
        public string? SpecialInstructions { get; set; }

        // Navigation Properties
        public virtual ICollection<LabService> LabServices { get; set; } = new HashSet<LabService>();
        public virtual ICollection<LabPrescriptionItem> PrescriptionItems { get; set; } = new HashSet<LabPrescriptionItem>();
        public virtual ICollection<LabResult> LabResults { get; set; } = new HashSet<LabResult>();
    }
}
