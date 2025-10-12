using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
    public class CreateLabPrescriptionItemRequest
    {
        public Guid LabTestId { get; set; }
        public string? SpecialInstructions { get; set; }
    }
}
