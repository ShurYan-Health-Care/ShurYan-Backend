using Shuryan.Core.Entities.Base;
using System;

namespace Shuryan.Core.Entities.External.Pharmacies
{
    public class PrescriptionStatusHistory : AuditableEntity
    {
        public Guid PrescriptionId { get; set; }
        public Prescription Prescription { get; set; } = null!;

        public string PreviousStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;

        public DateTime ChangedAt { get; set; }
        public Guid? ChangedBy { get; set; }
        public string? ChangedByName { get; set; }
        public string? ChangedByRole { get; set; }

        public string? Reason { get; set; }
        public string? Notes { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? AdditionalMetadata { get; set; }
    }
}
