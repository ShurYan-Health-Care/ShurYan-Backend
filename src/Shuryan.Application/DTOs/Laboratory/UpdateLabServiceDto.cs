using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class UpdateLabServiceDto
    {
        public decimal? Price { get; set; }
        public bool? IsAvailable { get; set; }
        public string? LabSpecificNotes { get; set; }
    }
}
