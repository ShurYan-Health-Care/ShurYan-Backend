using System;
using System.ComponentModel.DataAnnotations.Schema;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Pharmacy;

namespace Shuryan.Core.Entities.Shared
{
    public class PharmacyDocument : AuditableEntity
	{
        [ForeignKey("Pharmacy")]
        public Guid PharmacyId { get; set; }

        public string DocumentUrl { get; set; } = string.Empty;
        public PharmacyDocumentType Type { get; set; }
        public VerificationDocumentStatus Status { get; set; } = VerificationDocumentStatus.UnderReview;
		public string? RejectionReason { get; set; }

        // Navigation Properties
        public virtual Pharmacy Pharmacy { get; set; } = null!;
    }
}
