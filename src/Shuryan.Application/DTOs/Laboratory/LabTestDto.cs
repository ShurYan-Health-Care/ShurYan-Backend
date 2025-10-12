using Shuryan.Application.DTOs.Base;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class LabTestDto : BaseAuditableDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public LabTestCategory Category { get; set; } 
        public string? SpecialInstructions { get; set; }
    }


}
