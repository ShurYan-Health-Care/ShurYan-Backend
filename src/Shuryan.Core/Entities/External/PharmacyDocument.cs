using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Entities.External
{
    /// <summary>
    /// بيمثل مستند توثيق خاص بالصيدلية (زي الترخيص أو السجل التجاري)
    /// </summary>
    public class PharmacyDocument
    {
        public Guid Id { get; set; }
        public string DocumentUrl { get; set; } = string.Empty;
        public PharmacyDocumentType Type { get; set; }
        public VerificationDocumentStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime UploadedAt { get; set; }
        [ForeignKey("Pharmacy")]
        public Guid PharmacyId { get; set; }
        public virtual Pharmacy Pharmacy { get; set; } = null!;
    }
}
