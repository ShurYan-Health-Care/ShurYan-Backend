using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Medical
{
	/// <summary>
	/// ده جدول بنحط فيه أشهر أنواع التحاليل (Master Data) 
	/// عشان الدكتور يختار من قائمة جاهزة بدل ما يكتب free text
	/// </summary>
	public class LabTest
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty; // اسم التحليل (مثال: "صورة دم كاملة") إلخ
		public string Code { get; set; } = string.Empty; // كود التحليل : CBC, HbA1c
		public LabTestCategory Category { get; set; } // التحليل ده تبع قسم ايه ؟ (دم - سكر - كبد) إلخ
		public string? SpecialInstructions { get; set; } // تعليمات خاصة (مثلاً: "صيام 4 ساعات") بنحطها علي نوع تحليل معين
		public bool IsActive { get; set; } = true; // soft delete (التحليل لسه متاح ولا لا؟)
		public DateTime CreatedAt { get; set; }  // نوع التحليل ده اتضاف فالسيستم امته ؟

		// Navigation Properties

		// علاقة One-to-Many مع LabService:
		// التحليل الواحد ممكن يتقدم في كذا معمل (كل معمل له سعر/شروط مختلفة) 
		public virtual ICollection<LabService> LabServices { get; set; } = new HashSet<LabService>();
		// علاقة One-to-Many مع LabPrescriptionItem:
		// التحليل الواحد ممكن يطلبه دكاتره مختلفين في روشتات كتير
		public virtual ICollection<LabPrescriptionItem> PrescriptionItems { get; set; } = new HashSet<LabPrescriptionItem>();

		public virtual ICollection<LabResult> LabResults { get; set; } = new HashSet<LabResult>();

	}
}
