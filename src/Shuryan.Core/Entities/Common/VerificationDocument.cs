using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;
using System.Xml.Linq;
using Shuryan.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuryan.Core.Entities.Common
{
    public class VerificationDocument
    {
        public Guid Id { get; set; }
        public string DocumentUrl { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public DateTime UploadedAt { get; set; }
        public VerificationDocumentStatus Status { get; set; } = VerificationDocumentStatus.Pending;
        public string? RejectionReason { get; set; }

        [ForeignKey("Doctor")]
        public Guid DoctorId { get; set; }


        // Navigation Properties
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
