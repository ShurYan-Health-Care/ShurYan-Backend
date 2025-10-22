using Shuryan.Core.Entities.Base;
using System;

namespace Shuryan.Core.Entities.External.Pharmacies
{
    public class PrescriptionShare : AuditableEntity
    {
        public Guid PrescriptionId { get; set; }
        public Prescription Prescription { get; set; } = null!;

        public string ShareCode { get; set; } = string.Empty;
        public string ShareUrl { get; set; } = string.Empty;

        public DateTime SharedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public Guid? PharmacyId { get; set; }
        public int AccessCount { get; set; }
        public DateTime? LastAccessedAt { get; set; }

        public bool AllowMultipleViews { get; set; }
        public bool IsActive { get; set; }
        public bool IsRevoked { get; set; }
        public string? RevocationReason { get; set; }
        public DateTime? RevokedAt { get; set; }

        public string? MessageToPharmacy { get; set; }
    }
}
