using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Common
{
	public class MedicalHistoryItem
	{
		public Guid Id { get; set; }
		public MedicalHistoryType Type { get; set; }
		public string Text { get; set; } = string.Empty;
		public DateTime RecordedAt { get; set; }

		[ForeignKey("Patient")]
		public Guid PatientId { get; set; }

		// Navigation Properties
		public virtual Patient Patient { get; set; } = null!;
	}
}
