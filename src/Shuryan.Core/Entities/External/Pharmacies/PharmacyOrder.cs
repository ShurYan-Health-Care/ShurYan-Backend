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
    /// <summary>
    /// بيمثل طلب شراء أدوية من صيدلية معينة
    /// </summary>
    public class PharmacyOrder : AuditableEntity
	{
        public string OrderNumber { get; set; }
        public PharmacyOrderStatus Status { get; set; } = PharmacyOrderStatus.PendingPayment;
        public decimal TotalCost { get; set; }
        public decimal DeliveryFee { get; set; }
        public OrderDeliveryType DeliveryType { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public string DeliveryPersonPhone { get; set; } // د الرقم اللي العميل هيتواصل بيه مع بتاع الدليفري
        public string? DeliveryPersonName { get; set; } // اسم بتاع الدليفري
        public string? DeliveryNotes { get; set; } // معلومات عن التوصيل زي مثلا حط الطلب قدام الباب وصوره خبط مرتين
        public DateTime? ActualDeliveryTime { get; set; }


        [ForeignKey("Patient")]
        public Guid PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;

        [ForeignKey("Pharmacy")]
        public Guid PharmacyId { get; set; }
        public virtual Pharmacy Pharmacy { get; set; } = null!;

        [ForeignKey("Prescription")]
        public Guid? PrescriptionId { get; set; }
        public virtual Prescription? Prescription { get; set; }

		public virtual PharmacyReview? PharmacyReview { get; set; }

	}

}
