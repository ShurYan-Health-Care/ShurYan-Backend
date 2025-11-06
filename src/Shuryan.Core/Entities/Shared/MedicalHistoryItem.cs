using System;
using System.ComponentModel.DataAnnotations.Schema;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Shared
{
	public class MedicalHistoryItem : AuditableEntity
	{
		[ForeignKey("Patient")]
		public Guid PatientId { get; set; }

		public MedicalHistoryType Type { get; set; }
		public string Text { get; set; } = string.Empty;

		// Navigation Properties
		public virtual Patient Patient { get; set; } = null!;
	}
}
