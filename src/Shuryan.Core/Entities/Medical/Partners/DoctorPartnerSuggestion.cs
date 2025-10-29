using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.Core.Entities.Medical.Partners
{
    /// <summary>
    /// الكلاس ده مسؤول عن تخزين الشركاء المقترحين من الدكتور (صيدلية و/أو معمل)
    /// الدكتور ممكن يقترح صيدلية واحدة ومعمل واحد في نفس الوقت
    /// </summary>
    public class DoctorPartnerSuggestion : AuditableEntity
    {
        [ForeignKey("Doctor")]
        public Guid DoctorId { get; set; }

        /// <summary>
        /// ID الصيدلية المقترحة (nullable - ممكن يكون null لو مفيش صيدلية مقترحة)
        /// </summary>
        public Guid? SuggestedPharmacyId { get; set; }

        /// <summary>
        /// تاريخ اقتراح الصيدلية
        /// </summary>
        public DateTime? PharmacySuggestedAt { get; set; }

        /// <summary>
        /// ID المعمل المقترح (nullable - ممكن يكون null لو مفيش معمل مقترح)
        /// </summary>
        public Guid? SuggestedLaboratoryId { get; set; }

        /// <summary>
        /// تاريخ اقتراح المعمل
        /// </summary>
        public DateTime? LaboratorySuggestedAt { get; set; }

        // Navigation Properties
        public virtual Doctor Doctor { get; set; } = null!;
        
        // Note: We don't add navigation to Pharmacy/Laboratory here 
        // because it would create circular dependencies
        // The IDs will be used to fetch the partner data when needed
    }
}
