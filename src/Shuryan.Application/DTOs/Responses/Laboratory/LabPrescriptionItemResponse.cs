using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    public class LabPrescriptionItemResponse
    {
        public Guid LabTestId { get; set; }
        public string? SpecialInstructions { get; set; }
    }
}
