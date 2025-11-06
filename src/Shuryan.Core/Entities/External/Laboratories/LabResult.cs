using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;

namespace Shuryan.Core.Entities.External.Laboratories
{
    public class LabResult : AuditableEntity
    {
        [ForeignKey("LabOrder")]
        public Guid LabOrderId { get; set; }

        [ForeignKey("LabTest")]
        public Guid LabTestId { get; set; }

        public string ResultValue { get; set; } = string.Empty;
        public string? ReferenceRange { get; set; }
        public string? Unit { get; set; }
        public string? Notes { get; set; }
        public string? AttachmentUrl { get; set; }

        // Navigation Properties
        public virtual LabOrder LabOrder { get; set; } = null!;
        public virtual LabTest LabTest { get; set; } = null!;
    }
}
