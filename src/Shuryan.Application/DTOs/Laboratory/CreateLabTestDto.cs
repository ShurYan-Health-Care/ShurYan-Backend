using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class CreateLabTestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public LabTestCategory Category { get; set; }
        public string? SpecialInstructions { get; set; }
    }
}
