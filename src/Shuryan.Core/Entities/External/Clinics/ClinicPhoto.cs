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
        public string PhotoUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }

        [ForeignKey("Clinic")]
        public Guid ClinicId { get; set; }

        // Navigation Properties
        public virtual Clinic Clinic { get; set; } = null!;
    }
}
