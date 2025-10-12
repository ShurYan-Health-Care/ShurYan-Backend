using Shuryan.Application.DTOs.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Clinic
{
    public class ClinicPhoneNumberResponse : BaseAuditableDto
    {
        public string Number { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Guid ClinicId { get; set; }
    }
}
