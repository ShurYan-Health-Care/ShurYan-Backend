using Shuryan.Application.DTOs.Base;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class LaboratoryDocumentDto : BaseAuditableDto
    {
        public string DocumentUrl { get; set; } = string.Empty;
        public LaboratoryDocumentType Type { get; set; }
        public VerificationDocumentStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public Guid LaboratoryId { get; set; }
    }
}
