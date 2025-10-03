using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Identity
{
	public class Laboratory : User
	{
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public string? WhatsAppNumber { get; set; }
		public string? Website { get; set; }

		public LaboratoryStatus LaboratoryStatus { get; set; } = LaboratoryStatus.Active;

		public bool OffersHomeSampleCollection { get; set; } = false;
		public decimal? HomeSampleCollectionFee { get; set; }

		public LaboratoryVerificationStatus VerificationStatus { get; set; } = LaboratoryVerificationStatus.Unverified;
		public DateTime? VerifiedAt { get; set; }

		[ForeignKey("Verifier")]
		public Guid? VerifierId { get; set; }

		[ForeignKey("Address")]
		public Guid AddressId { get; set; }

		// Navigation Properties
		public virtual Verifier? Verifier { get; set; }
		public virtual Address Address { get; set; } = null!;
		public virtual ICollection<LaboratoryDocument> VerificationDocuments { get; set; } = new HashSet<LaboratoryDocument>();
		public virtual ICollection<LaboratoryWorkingHours> WorkingHours { get; set; } = new HashSet<LaboratoryWorkingHours>();
		public virtual ICollection<LabService> LabServices { get; set; } = new HashSet<LabService>();
		public virtual ICollection<LabOrder> LabOrders { get; set; } = new HashSet<LabOrder>();
	}
}
