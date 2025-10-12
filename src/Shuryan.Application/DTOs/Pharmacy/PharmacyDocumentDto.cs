using Shuryan.Application.DTOs.Base;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Pharmacy
{
    public class PharmacyDocumentDto : BaseAuditableDto
    {
        public string DocumentUrl { get; set; } = string.Empty;
        public PharmacyDocumentType Type { get; set; }
        public VerificationDocumentStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public Guid PharmacyId { get; set; }
    }
}
