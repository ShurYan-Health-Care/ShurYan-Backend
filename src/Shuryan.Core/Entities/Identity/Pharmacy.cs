using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.External;
using Shuryan.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuryan.Core.Entities.Identity
{
    /// <summary>
    /// بيمثل الصيدلية كـ "مستخدم" في النظام له حساب وصلاحيات
    /// </summary>
    public class Pharmacy : User
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Website { get; set; }
        public PharmacyStatus LaboratoryStatus { get; set; } = PharmacyStatus.Active;
        public bool OffersDelivery { get; set; } = true;
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Unverified;
        public DateTime? VerifiedAt { get; set; }

        [ForeignKey("Verifier")]
        public Guid? VerifierId { get; set; }

        [ForeignKey("Address")]
        public Guid AddressId { get; set; }

        // Navigation Properties
        public virtual Verifier? Verifier { get; set; }
        public virtual Address Address { get; set; } = null!;
        public virtual ICollection<PharmacyDocument> VerificationDocuments { get; set; } = new HashSet<PharmacyDocument>();
        public virtual ICollection<PharmacyWorkingHours> WorkingHours { get; set; } = new HashSet<PharmacyWorkingHours>();
        public virtual ICollection<PharmacyOrder> Orders { get; set; } = new HashSet<PharmacyOrder>();
    }
}