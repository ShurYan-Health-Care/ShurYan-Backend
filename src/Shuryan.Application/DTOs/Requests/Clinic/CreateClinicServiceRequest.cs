using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Clinic
{
    public class CreateClinicServiceRequest
    {
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
