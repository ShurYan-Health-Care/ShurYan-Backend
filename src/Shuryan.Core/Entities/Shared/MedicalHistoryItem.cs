using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Common
{
	public class MedicalHistoryItem : AuditableEntity
	{
		public MedicalHistoryType Type { get; set; }
		public string Text { get; set; } = string.Empty;

		[ForeignKey("Patient")]
		public Guid PatientId { get; set; }

		// Navigation Properties
		public virtual Patient Patient { get; set; } = null!;
	}
}
