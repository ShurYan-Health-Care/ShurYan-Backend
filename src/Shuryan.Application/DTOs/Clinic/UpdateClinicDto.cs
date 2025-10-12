using Shuryan.Application.DTOs.Common.Address;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Clinic
{
    public class UpdateClinicDto
    {
        public string? Name { get; set; }
        public string? FacilityVideoUrl { get; set; }
        public UpdateAddressDto? Address { get; set; }
    }

}
