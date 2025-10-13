using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Prescription
{
    public class UpdatePrescriptionRequest
    {
        public string? GeneralInstructions { get; set; }
        public string? FollowUpInstructions { get; set; }
    }
}

