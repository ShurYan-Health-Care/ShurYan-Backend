using Shuryan.Application.DTOs.Base;
using Shuryan.Core.Enums.Clinic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Clinic
{
    public class ClinicServiceDto : BaseAuditableDto
    {
        public ClinicServiceType ServiceType { get; set; }
        public Guid ClinicId { get; set; }
    }
}
