using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Medical.Schedules
{
    // الكلاس د ال هتبقى مسؤولة عن اضافة مواعيد الدكاترة الاساسية الثابته
    public class DoctorAvailability : SoftDeletableEntity
    {

        [ForeignKey("Doctor")]
        public Guid DoctorId { get; set; }

        public SysDayOfWeek DayOfWeek { get; set; }

        // We use TimeOnly to store only the time part
        public TimeOnly StartTime { get; set; } // The start time of the slot (ex: 10:00 AM)
        public TimeOnly EndTime { get; set; } // The end time of the slot (ex: 02:00 PM)

        // Navigation Properties
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
