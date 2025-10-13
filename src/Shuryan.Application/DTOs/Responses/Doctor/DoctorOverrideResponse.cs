using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    public class DoctorOverrideResponse : BaseAuditableDto
    {
        public Guid DoctorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public OverrideType Type { get; set; }
    }
}
