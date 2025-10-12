using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Appointment
{
    public class CancelAppointmentRequest
    {
        public string CancellationReason { get; set; } = string.Empty;
    }
}

