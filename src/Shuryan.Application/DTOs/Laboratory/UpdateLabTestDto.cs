using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class UpdateLabTestDto
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public LabTestCategory? Category { get; set; }
        public string? SpecialInstructions { get; set; }
    }
}
