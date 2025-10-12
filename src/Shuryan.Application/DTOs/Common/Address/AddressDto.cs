using Shuryan.Application.DTOs.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Common.Address
{
    public class AddressDto : BaseSoftDeletableDto
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public string? BuildingNumber { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}