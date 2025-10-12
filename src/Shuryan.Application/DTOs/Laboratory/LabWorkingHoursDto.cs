using Shuryan.Application.DTOs.Base;
using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class LabWorkingHoursDto : BaseAuditableDto
    {
        public Guid LaboratoryId { get; set; }
        public SysDayOfWeek Day { get; set; } 
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
    }

}
