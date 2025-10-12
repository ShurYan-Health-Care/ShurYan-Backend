using Shuryan.Application.DTOs.Base;
using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Pharmacy
{
    public class PharmacyWorkingHoursDto : BaseAuditableDto
    {
        public Guid PharmacyId { get; set; }
        public SysDayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
