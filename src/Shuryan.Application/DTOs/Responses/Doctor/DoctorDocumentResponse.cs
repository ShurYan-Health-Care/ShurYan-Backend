using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Doctor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    public class DoctorDocumentResponse : BaseAuditableDto
    {
        public string DocumentUrl { get; set; } = string.Empty;
        public DoctorDocumentType Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public VerificationDocumentStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
    }
}
