using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;
using System.Xml.Linq;
using Shuryan.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Entities.Base;

namespace Shuryan.Core.Entities.Common
{
    public class DoctorDocument : AuditableEntity
	{
		public string DocumentUrl { get; set; } = string.Empty;
        public DoctorDocumentType Type { get; set; }
        public VerificationDocumentStatus Status { get; set; } = VerificationDocumentStatus.UnderReview;
        public string? RejectionReason { get; set; }

        [ForeignKey("Doctor")]
        public Guid DoctorId { get; set; }


        // Navigation Properties
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
