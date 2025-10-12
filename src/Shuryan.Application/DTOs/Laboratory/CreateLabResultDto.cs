using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class CreateLabResultDto
    {
        public Guid LabTestId { get; set; }
        public string? ResultFileUrl { get; set; }
        public string? LabNotes { get; set; }
    }

}
