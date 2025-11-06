using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.Core.Entities.Medical.Partners
{
    public class DoctorPartnerSuggestion : AuditableEntity
    {
        [ForeignKey("Doctor")]
        public Guid DoctorId { get; set; }

        public Guid? SuggestedPharmacyId { get; set; }
        public DateTime? PharmacySuggestedAt { get; set; }

        public Guid? SuggestedLaboratoryId { get; set; }
        public DateTime? LaboratorySuggestedAt { get; set; }

        // Navigation Properties
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
