using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Enums.Clinic;

namespace Shuryan.Core.Entities.External.Clinic
{
    public class ClinicService : AuditableEntity
	{
        public ClinicServiceType ServiceType { get; set; }

        [ForeignKey("Clinic")]
        public Guid ClinicId { get; set; }

        // Navigation property to the Clinic
        public virtual Clinic Clinic { get; set; } = null!;
    }
}