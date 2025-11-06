using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;

namespace Shuryan.Core.Entities.External.Laboratories
{
    public class LabPrescriptionItem : AuditableEntity
    {
        [ForeignKey("LabPrescription")]
        public Guid LabPrescriptionId { get; set; }

        [ForeignKey("LabTest")]
        public Guid LabTestId { get; set; }

        public string? DoctorNotes { get; set; }

        // Navigation Properties
        public virtual LabPrescription LabPrescription { get; set; } = null!;
        public virtual LabTest LabTest { get; set; } = null!;
    }
}
