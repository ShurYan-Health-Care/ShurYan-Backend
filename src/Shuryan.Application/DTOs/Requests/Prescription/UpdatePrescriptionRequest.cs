using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Prescription
{
    public class UpdatePrescriptionRequest
    {
        [StringLength(1000, ErrorMessage = "General instructions cannot exceed 1000 characters")]
        public string? GeneralInstructions { get; set; }
    }
}

