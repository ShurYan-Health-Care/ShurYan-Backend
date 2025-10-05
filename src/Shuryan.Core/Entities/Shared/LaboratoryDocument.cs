using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Laboratory;

namespace Shuryan.Core.Entities.Shared
{
    public class LaboratoryDocument
    {
        public Guid Id { get; set; }
        [Required, MaxLength(500)]
        [RegularExpression(@"^https?://.*", ErrorMessage = "Must be a valid URL")]
        public string DocumentUrl { get; set; } = string.Empty;
        public LaboratoryDocumentType Type { get; set; }
        public DateTime UploadedAt { get; set; }
        public VerificationDocumentStatus Status { get; set; } = VerificationDocumentStatus.Pending;
        public string? RejectionReason { get; set; }

        [ForeignKey("Laboratory")]
        public Guid LaboratoryId { get; set; }

        // Navigation Properties
        public virtual Laboratory Laboratory { get; set; } = null!;
    }
}
