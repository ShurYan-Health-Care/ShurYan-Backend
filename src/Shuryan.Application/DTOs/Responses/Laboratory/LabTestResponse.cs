using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    public class LabTestResponse : BaseAuditableDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public LabTestCategory Category { get; set; }
        public string? SpecialInstructions { get; set; }
    }
}
