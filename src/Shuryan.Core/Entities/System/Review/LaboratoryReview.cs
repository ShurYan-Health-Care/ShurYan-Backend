using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Base;

namespace Shuryan.Core.Entities.System.Review
{
    public class LaboratoryReview : AuditableEntity
	{
		[ForeignKey("LabOrder")]
		public Guid LabOrderId { get; set; }

		[ForeignKey("Patient")]
		public Guid PatientId { get; set; }

		[ForeignKey("Laboratory")]
		public Guid LaboratoryId { get; set; }

		[Range(1, 5)]
		public int OverallSatisfaction { get; set; } // الرضا العام

		[Range(1, 5)]
		public int ResultAccuracy { get; set; } // دقة النتائج

		[Range(1, 5)]
		public int DeliverySpeed { get; set; } // سرعة التسليم

		[Range(1, 5)]
		public int ServiceQuality { get; set; } // جودة الخدمة

		[Range(1, 5)]
		public int ValueForMoney { get; set; } // القيمة مقابل السعر

		public bool IsEdited { get; set; } = false;

		// Navigation Properties
		public virtual LabOrder LabOrder { get; set; } = null!;
		public virtual Patient Patient { get; set; } = null!;
		public virtual Laboratory Laboratory { get; set; } = null!;

		// Computed Property
		[NotMapped]
		public double AverageRating => (OverallSatisfaction + ResultAccuracy + DeliverySpeed + ServiceQuality + ValueForMoney) / 5.0;
	}
}