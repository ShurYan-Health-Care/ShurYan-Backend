using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Entities.External
{
    /// <summary>
    /// بيمثل طلب شراء أدوية من صيدلية معينة
    /// </summary>
    public class PharmacyOrder
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public PharmacyOrderStatus Status { get; set; }
        public decimal TotalCost { get; set; }
        public decimal DeliveryFee { get; set; }
        public OrderDeliveryType DeliveryType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EstimatedDeliveryTime { get; set; }
        public string DeliveryPersonPhone { get; set; } // د الرقم اللي العميل هيتواصل بيه مع بتاع الدليفري
        public string DeliveryPersonName { get; set; } // اسم بتاع الدليفري
        public string? DeliveryNotes { get; set; } // معلومات عن التوصيل زي مثلا حط الطلب قدام الباب وصوره خبط مرتين

        public PharmacyPaymentMethod PaymentMethod { get; set; }
        // --- العلاقات ---
        [ForeignKey("Patient")]
        public Guid PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;

        [ForeignKey("Pharmacy")]
        public Guid PharmacyId { get; set; }
        public virtual Pharmacy Pharmacy { get; set; } = null!;

        [ForeignKey("MedicationPrescription")]
        public Guid MedicationPrescriptionId { get; set; }
        public virtual MedicationPrescription MedicationPrescription { get; set; } = null!;
    }

}
