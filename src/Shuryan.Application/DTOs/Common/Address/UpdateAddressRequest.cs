using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Enums;

namespace Shuryan.Application.DTOs.Common.Address
{
    public class UpdateAddressRequest
    {
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Street must be between 2-200 characters")]
        public string? Street { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "City must be between 2-100 characters")]
        public string? City { get; set; }

        public Governorate? Governorate { get; set; }

        [StringLength(20, ErrorMessage = "Building number cannot exceed 20 characters")]
        public string? BuildingNumber { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double? Longitude { get; set; }
    }
}
