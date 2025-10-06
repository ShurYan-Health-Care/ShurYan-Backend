using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Appointments;

namespace Shuryan.Core.Entities.Medical.Schedules
{
    // الكلاس د ال هتبقى مسؤولة عن اضافة او حذف مواعيد استثنائية ف ايام معينة
    public class DoctorOverride : AuditableEntity
	{

        [ForeignKey("Doctor")]
        public Guid DoctorId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public OverrideType Type { get; set; } // Available or Unavailable


        // Navigation Properties
        public virtual Doctor Doctor { get; set; } = null!;
    }
}