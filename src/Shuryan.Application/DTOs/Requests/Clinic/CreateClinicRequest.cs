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
    public class CreateClinicRequest
    {
        [Required(ErrorMessage = "Clinic name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Clinic name must be between 3-200 characters")]
        public string Name { get; set; } = string.Empty;

        public IFormFile? FacilityVideo { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public CreateAddressRequest Address { get; set; } = null!;

        public IEnumerable<string> PhoneNumbers { get; set; } = new List<string>();

        public IEnumerable<string> Services { get; set; } = new List<string>();
    }
}
