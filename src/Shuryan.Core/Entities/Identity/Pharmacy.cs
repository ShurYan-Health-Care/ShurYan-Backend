using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Core.Enums.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuryan.Core.Entities.Identity
{
    public class Pharmacy : User
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Website { get; set; }
        public Status PharmacyStatus { get; set; } = Status.Active;
        public bool OffersDelivery { get; set; } = true;
        public decimal DeliveryFee { get; set; } = 0;
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Unverified;
        public DateTime? VerifiedAt { get; set; }

        [ForeignKey("Verifier")]
        public Guid? VerifierId { get; set; }

        [ForeignKey("Address")]
        public Guid? AddressId { get; set; }

        // Navigation Properties
        public virtual Verifier? Verifier { get; set; }
        public virtual Address? Address { get; set; }
        public virtual ICollection<PharmacyDocument> VerificationDocuments { get; set; } = new HashSet<PharmacyDocument>();
        public virtual ICollection<PharmacyWorkingHours> WorkingHours { get; set; } = new HashSet<PharmacyWorkingHours>();
        public virtual ICollection<PharmacyOrder> Orders { get; set; } = new HashSet<PharmacyOrder>();
		public virtual ICollection<PharmacyReview> PharmacyReviews { get; set; } = new HashSet<PharmacyReview>();

		[NotMapped]
		public double? AverageRating { get; set; }

		[NotMapped]
		public int TotalReviewsCount { get; set; }
	}
}