using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class CreateLabServiceDto
    {
        public Guid LabTestId { get; set; }
        public decimal Price { get; set; }
        public string? LabSpecificNotes { get; set; }
    }

}
