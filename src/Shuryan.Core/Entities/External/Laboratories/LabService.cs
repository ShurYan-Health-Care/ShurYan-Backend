using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.External;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Core.Entities.External.Laboratories
{
    public class LabService : AuditableEntity
    {
        [ForeignKey("Laboratory")]
        public Guid LaboratoryId { get; set; }

        [ForeignKey("LabTest")]
        public Guid LabTestId { get; set; }

        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? LabSpecificNotes { get; set; }

        // Navigation Properties
        public virtual Laboratory Laboratory { get; set; } = null!;
        public virtual LabTest LabTest { get; set; } = null!;
    }
}
