using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Core.Entities.System.Review
{
    public class PharmacyReview : AuditableEntity
	{

		[ForeignKey("PharmacyOrder")]
		public Guid PharmacyOrderId { get; set; }

		[ForeignKey("Patient")]
		public Guid PatientId { get; set; }

		[ForeignKey("Pharmacy")]
		public Guid PharmacyId { get; set; }

		[Range(1, 5)]
		public int OverallSatisfaction { get; set; } // الرضا العام

		[Range(1, 5)]
		public int MedicationAvailability { get; set; } // توفر الأدوية

		[Range(1, 5)]
		public int ServiceQuality { get; set; } // جودة الخدمة

		[Range(1, 5)]
		public int DeliverySpeed { get; set; } // سرعة التوصيل

		[Range(1, 5)]
		public int ValueForMoney { get; set; } // القيمة مقابل السعر

		public bool IsEdited { get; set; } = false;

		// Navigation Properties
		public virtual PharmacyOrder PharmacyOrder { get; set; } = null!;
		public virtual Patient Patient { get; set; } = null!;
		public virtual Pharmacy Pharmacy { get; set; } = null!;

		[NotMapped]
		public double AverageRating => (OverallSatisfaction + MedicationAvailability + ServiceQuality + DeliverySpeed + ValueForMoney) / 5.0;
	}
}
