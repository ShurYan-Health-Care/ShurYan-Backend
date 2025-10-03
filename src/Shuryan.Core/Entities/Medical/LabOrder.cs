using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.External;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Medical
{
	public class LabOrder
	{
		public Guid Id { get; set; }

		[ForeignKey("LabPrescription")]
		public Guid LabPrescriptionId { get; set; } // الروشته ال مطلوب ليها تحاليل

		[ForeignKey("Laboratory")]
		public Guid LaboratoryId { get; set; } // مين المعمل اللي هينفذ الطلب ؟

		[ForeignKey("Patient")]
		public Guid PatientId { get; set; } // الطلب خاص باي مريض ؟

		public LabOrderStatus Status { get; set; } = LabOrderStatus.PendingPayment; // حالة الطلب

		// طريقة جمع العينة (من البيت أو من المعمل)
		public SampleCollectionType SampleCollectionType { get; set; } = SampleCollectionType.LabVisit; // هنستلم العينه منك فالبيت ولا فالمعمل

		// التكلفة
		public decimal TestsTotalCost { get; set; } // مجموع أسعار التحاليل نفسها
		public decimal SampleCollectionDeliveryCost { get; set; } = 0; // تكلفة جمع العينة من البيت لو موجودة
		
		[NotMapped]
		public decimal TotalCost => TestsTotalCost + SampleCollectionDeliveryCost;

		// التأكيد
		public DateTime? ConfirmedByLabAt { get; set; }

		// الإلغاء/الرفض
		public string? CancellationReason { get; set; } // سبب إلغاء الطلب (لو المريض أو المعمل لغاه)
		public DateTime? CancelledAt { get; set; } // وقت الإلغاء

		// Navigation Properties
		public virtual LabPrescription LabPrescription { get; set; } = null!;
		public virtual Laboratory Laboratory { get; set; } = null!;
		public virtual Patient Patient { get; set; } = null!;
		public virtual ICollection<LabResult> LabResults { get; set; } = new HashSet<LabResult>();

	}
}
