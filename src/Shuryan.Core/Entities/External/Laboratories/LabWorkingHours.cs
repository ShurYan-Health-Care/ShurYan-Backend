using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Core.Entities.External.Laboratories
{
    public class LabWorkingHours : AuditableEntity
    {
        [ForeignKey("Laboratory")]
        public Guid LaboratoryId { get; set; }

        public DayOfWeek Day { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Laboratory Laboratory { get; set; } = null!;
    }
}
