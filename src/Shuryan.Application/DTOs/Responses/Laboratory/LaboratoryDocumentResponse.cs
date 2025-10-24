using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    public class LaboratoryDocumentResponse : BaseAuditableDto
    {
        public string DocumentUrl { get; set; } = string.Empty;
        public LaboratoryDocumentType Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public VerificationDocumentStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public Guid LaboratoryId { get; set; }
        public string LaboratoryName { get; set; } = string.Empty;
    }
}
