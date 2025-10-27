using Microsoft.AspNetCore.Http;
using Shuryan.Application.DTOs.Common.Address;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Clinic
{
    public class UpdateClinicRequest
    {
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Clinic name must be between 3-200 characters")]
        public string? Name { get; set; }

        public IFormFile? FacilityVideo { get; set; }

        public UpdateAddressRequest? Address { get; set; }
    }
}