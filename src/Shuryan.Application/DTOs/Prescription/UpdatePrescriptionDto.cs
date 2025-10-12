using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Prescription
{
    public class UpdatePrescriptionDto
    {
        public string? GeneralInstructions { get; set; }
        public string? FollowUpInstructions { get; set; }
    }

}
