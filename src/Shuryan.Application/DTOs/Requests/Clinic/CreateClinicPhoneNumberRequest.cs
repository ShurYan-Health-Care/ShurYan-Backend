using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Clinic
{
    public class CreateClinicPhoneNumberRequest
    {
        public string Number { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
