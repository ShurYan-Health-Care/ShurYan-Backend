using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
    public class CreateLabServiceRequest
    {
        public Guid LabTestId { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? LabSpecificNotes { get; set; }
    }
}
