using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Clinic
{
    public class CreateClinicPhoneNumberRequest
    {
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10-20 characters")]
        public string Number { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone type is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Phone type must be between 2-50 characters")]
        public string Type { get; set; } = string.Empty;
    }
}
