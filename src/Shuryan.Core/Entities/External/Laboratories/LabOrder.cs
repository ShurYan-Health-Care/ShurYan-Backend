using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.External;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Core.Enums.Laboratory;

namespace Shuryan.Core.Entities.External.Laboratories
{
    public class LabOrder : AuditableEntity
    {
        [ForeignKey("LabPrescription")]
        public Guid LabPrescriptionId { get; set; }

        [ForeignKey("Laboratory")]
        public Guid LaboratoryId { get; set; }

        [ForeignKey("Patient")]
        public Guid PatientId { get; set; }

        public LabOrderStatus Status { get; set; } = LabOrderStatus.NewRequest;
        public SampleCollectionType SampleCollectionType { get; set; } = SampleCollectionType.LabVisit;
        public decimal TestsTotalCost { get; set; }
        public decimal SampleCollectionDeliveryCost { get; set; } = 0;
        public DateTime? ConfirmedByLabAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? SamplesCollectedAt { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? RejectedAt { get; set; }

        // Navigation Properties
        public virtual LabPrescription LabPrescription { get; set; } = null!;
        public virtual Laboratory Laboratory { get; set; } = null!;
        public virtual Patient Patient { get; set; } = null!;
        public virtual ICollection<LabResult> LabResults { get; set; } = new HashSet<LabResult>();
        public virtual LaboratoryReview? LaboratoryReview { get; set; }
    }
}
