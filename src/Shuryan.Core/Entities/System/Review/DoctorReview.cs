using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical.Appointments;
using Shuryan.Core.Entities.Base;

namespace Shuryan.Core.Entities.System.Review
{
    public class DoctorReview : AuditableEntity
	{

		[ForeignKey("Appointment")]
		public Guid AppointmentId { get; set; }

		[ForeignKey("Patient")]
		public Guid PatientId { get; set; }

		[ForeignKey("Doctor")]
		public Guid DoctorId { get; set; }

		[Range(1, 5)]
		public int OverallSatisfaction { get; set; } // الرضا العام

		[Range(1, 5)]
		public int WaitingTime { get; set; } // وقت الانتظار

		[Range(1, 5)]
		public int CommunicationQuality { get; set; } // جودة التواصل

		[Range(1, 5)]
		public int ClinicCleanliness { get; set; } // نظافة العيادة

		[Range(1, 5)]
		public int ValueForMoney { get; set; } // القيمة مقابل السعر

		// التعليق (اختياري)
		[MaxLength(500)]
		public string? Comment { get; set; }

		// هل التقييم منشور كمجهول؟
		public bool IsAnonymous { get; set; } = false;

		public bool IsEdited { get; set; } = false; // هل تم تعديل التقييم؟

		// Navigation Properties
		public virtual Appointment Appointment { get; set; } = null!;
		public virtual Patient Patient { get; set; } = null!;
		public virtual Doctor Doctor { get; set; } = null!;

		// For the future: Reply of Dr.
		[MaxLength(300)]
		public string? DoctorReply { get; set; }
		public DateTime? DoctorRepliedAt { get; set; }

		// Computed Property
		[NotMapped]
		public double AverageRating => (OverallSatisfaction + WaitingTime + CommunicationQuality + ClinicCleanliness + ValueForMoney) / 5.0;
	}
}