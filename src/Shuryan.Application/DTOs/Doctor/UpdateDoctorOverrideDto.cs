using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Doctor
{
    public class UpdateDoctorOverrideDto
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public OverrideType? Type { get; set; }
    }
}
