using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;

namespace Shuryan.Core.Entities.External.Clinic
{
    public class ClinicPhoto : AuditableEntity
	{

        [Required, MaxLength(500)]
        [RegularExpression(@"^https?://.*", ErrorMessage = "Must be a valid URL")]
        public string PhotoUrl { get; set; } = string.Empty;

        /// <summary>
        /// ترتيب الصورة (من 0 لـ 5) - الحد الأقصى 6 صور
        /// </summary>
        [Range(0, 5, ErrorMessage = "الترتيب لازم يكون بين 0 و 5")]
        public int DisplayOrder { get; set; }

        // Foreign Keys
        [ForeignKey("Clinic")]
        public Guid ClinicId { get; set; }

        // Navigation Properties
        public virtual Clinic Clinic { get; set; } = null!;
    }
}
