using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Core.Entities.Medical
{
	/// <summary>
	/// دي الروشتة (Prescription) اللي الدكتور بيكتبها للمريض
	/// بتكون مرتبطة ب Appointment (كشف/سيشن) 
	/// وبيكون فيها لستة بالتحاليل المطلوبة
	/// </summary>
	public class LabPrescription
	{
		public Guid Id { get; set; }

		[ForeignKey("Appointment")]
		public Guid AppointmentId { get; set; } // الروشتة دي اتكتبت فين؟ مرتبطة بكشف أو سيشن معينة.

		[ForeignKey("Doctor")]
		public Guid DoctorId { get; set; } // مين الدكتور اللي كتب الروشتة دي؟

		[ForeignKey("Patient")]
		public Guid PatientId { get; set; } // الروشتة دي تخص أي مريض؟

		public string? GeneralNotes { get; set; } // ملاحظات عامة من الدكتور علي الروشته كامله

		public DateTime CreatedAt { get; set; }  // الروشته اتعملت امته ؟

		// Navigation Properties
		public virtual Appointment Appointment { get; set; } = null!; // علاقة One-to-One: الروشتة دي مرتبطة بحجز واحد بس.
		public virtual Doctor Doctor { get; set; } = null!; // علاقة Many-to-One: دكتور واحد بيكتب الروشتة، لكن الدكتور ممكن يكتب روشتات كتير.
		public virtual Patient Patient { get; set; } = null!; // علاقة Many-to-One: الروشتة تخص مريض واحد، لكن المريض ممكن يكون عنده أكتر من روشتة.
		public virtual ICollection<LabPrescriptionItem> Items { get; set; } = new HashSet<LabPrescriptionItem>(); // علاقة One-to-Many: الروشتة فيها لستة تحاليل (Items)

		/// <summary>
		/// علاقة One-to-One (Optional): 
		/// الروشتة ممكن يتعمل منها Order (طلب رسمي للمعمل)
		/// وممكن لسه متعملش طلب
		/// </summary>
		public virtual LabOrder? LabOrder { get; set; } // الطلب المرتبط بهذه الروشتة
	}
}
