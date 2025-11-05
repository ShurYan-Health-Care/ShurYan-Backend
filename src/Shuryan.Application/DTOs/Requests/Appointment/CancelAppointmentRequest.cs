using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Appointment
{
    public class CancelAppointmentRequest
    {
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Cancellation reason must be between 5-500 characters")]
        public string? CancellationReason { get; set; }
    }
}

