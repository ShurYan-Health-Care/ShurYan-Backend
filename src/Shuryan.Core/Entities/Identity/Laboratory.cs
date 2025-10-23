using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums.Laboratory;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Entities.System.Review;

namespace Shuryan.Core.Entities.Identity
{
    public class Laboratory : User
	{
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public string? WhatsAppNumber { get; set; }
		public string? Website { get; set; }

		public Status LaboratoryStatus { get; set; } = Status.Active;

		public bool OffersHomeSampleCollection { get; set; } = false;
		public decimal? HomeSampleCollectionFee { get; set; }

		public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Unverified;
		public DateTime? VerifiedAt { get; set; }

		[ForeignKey("Verifier")]
		public Guid? VerifierId { get; set; }

		[ForeignKey("Address")]
		public Guid? AddressId { get; set; }

		// Navigation Properties
		public virtual Verifier? Verifier { get; set; }
        public virtual Address? Address { get; set; }
        public virtual ICollection<LaboratoryDocument> VerificationDocuments { get; set; } = new HashSet<LaboratoryDocument>();
		public virtual ICollection<LabWorkingHours> WorkingHours { get; set; } = new HashSet<LabWorkingHours>();
		public virtual ICollection<LabService> LabServices { get; set; } = new HashSet<LabService>();
		public virtual ICollection<LabOrder> LabOrders { get; set; } = new HashSet<LabOrder>();
		public virtual ICollection<LaboratoryReview> LaboratoryReviews { get; set; } = new HashSet<LaboratoryReview>();

		[NotMapped]
		public double? AverageRating { get; set; }

		[NotMapped]
		public int TotalReviewsCount { get; set; }
	}
}
