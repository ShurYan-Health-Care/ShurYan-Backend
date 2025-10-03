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
	public class LaboratoryDocument
	{
		public Guid Id { get; set; }
		public string DocumentUrl { get; set; } = string.Empty;
		public LaboratoryDocumentType Type { get; set; }
		public DateTime UploadedAt { get; set; }
		public LaboratoryDocumentStatus Status { get; set; } = LaboratoryDocumentStatus.Pending;
		public string? RejectionReason { get; set; }

		[ForeignKey("Laboratory")]
		public Guid LaboratoryId { get; set; }

		// Navigation Properties
		public virtual Laboratory Laboratory { get; set; } = null!;
	}
}
