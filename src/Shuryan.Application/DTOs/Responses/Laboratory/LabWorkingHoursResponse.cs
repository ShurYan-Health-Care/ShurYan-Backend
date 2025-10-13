using Shuryan.Application.DTOs.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    public class LabWorkingHoursResponse : BaseAuditableDto
    {
        public Guid LaboratoryId { get; set; }
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
    }
}
