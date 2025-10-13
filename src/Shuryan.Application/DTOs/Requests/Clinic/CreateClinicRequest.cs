using Shuryan.Application.DTOs.Common.Address;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Clinic
{
    public class CreateClinicRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? FacilityVideoUrl { get; set; }
        public CreateAddressDto Address { get; set; } = null!;
        public IEnumerable<string> PhoneNumbers { get; set; } = new List<string>();
        public IEnumerable<string> Services { get; set; } = new List<string>();
    }
}
