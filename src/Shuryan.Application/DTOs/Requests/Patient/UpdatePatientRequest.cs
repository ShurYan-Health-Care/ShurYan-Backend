using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Core.Enums.Identity;

namespace Shuryan.Application.DTOs.Requests.Patient
{
    public class UpdatePatientRequest
    {
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2-50 characters")]
        public string? FirstName { get; set; }

        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2-50 characters")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10-20 characters")]
        public string? PhoneNumber { get; set; }

        public DateTime? BirthDate { get; set; }

        public Gender? Gender { get; set; }

        [Url(ErrorMessage = "Invalid profile image URL format")]
        public string? ProfileImageUrl { get; set; }

        public UpdateAddressRequest? Address { get; set; }
    }
}

