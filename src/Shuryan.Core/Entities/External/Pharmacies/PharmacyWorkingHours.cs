using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.External.Pharmacies
{
    public class PharmacyWorkingHours : SoftDeletableEntity
	{
        public SysDayOfWeek DayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        [ForeignKey("Pharmacy")]
        public Guid PharmacyId { get; set; }
        public virtual Pharmacy Pharmacy { get; set; } = null!;
    }
}
