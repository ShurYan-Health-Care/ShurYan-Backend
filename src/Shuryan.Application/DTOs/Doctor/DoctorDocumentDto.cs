using Shuryan.Application.DTOs.Base;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Doctor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Doctor
{
    public class DoctorDocumentDto : BaseAuditableDto
    {
        public string DocumentUrl { get; set; } = string.Empty;
        public DoctorDocumentType Type { get; set; }
        public VerificationDocumentStatus Status { get; set; } 
        public string? RejectionReason { get; set; }
        public Guid DoctorId { get; set; }
    }


}
