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

        // Foreign Keys
        [ForeignKey("Clinic")]
        public Guid ClinicId { get; set; }

        // Navigation Properties
        public virtual Clinic Clinic { get; set; } = null!;
    }
}
