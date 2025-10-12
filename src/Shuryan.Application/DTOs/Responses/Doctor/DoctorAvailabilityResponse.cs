using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    public class DoctorAvailabilityResponse : BaseSoftDeletableDto
    {
        public Guid DoctorId { get; set; }
        public SysDayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}

