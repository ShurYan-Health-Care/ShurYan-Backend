using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Entities.External.Pharmacies
{
    public class PharmacyOrder : AuditableEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public PharmacyOrderStatus Status { get; set; } = PharmacyOrderStatus.PendingPharmacyResponse;
        public decimal TotalCost { get; set; }
        public decimal DeliveryFee { get; set; }
        public OrderDeliveryType DeliveryType { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public string DeliveryPersonPhone { get; set; } = string.Empty;
        public string? DeliveryPersonName { get; set; }
        public string? DeliveryNotes { get; set; }
        public DateTime? ActualDeliveryTime { get; set; }

        // Patient Confirmation
        public bool? PatientConfirmed { get; set; }
        public DateTime? PatientConfirmedAt { get; set; }
        public string? PatientNotes { get; set; }
        public string? PatientDigitalSignature { get; set; }

        [ForeignKey("Patient")]
        public Guid PatientId { get; set; }

        [ForeignKey("Pharmacy")]
        public Guid PharmacyId { get; set; }

        [ForeignKey("Prescription")]
        public Guid? PrescriptionId { get; set; }

        // Navigation Properties
        public virtual Patient Patient { get; set; } = null!;
        public virtual Pharmacy Pharmacy { get; set; } = null!;
        public virtual Prescription? Prescription { get; set; }
        public virtual PharmacyReview? PharmacyReview { get; set; }
        public virtual ICollection<PharmacyOrderItem> OrderItems { get; set; } = new HashSet<PharmacyOrderItem>();
    }

}
