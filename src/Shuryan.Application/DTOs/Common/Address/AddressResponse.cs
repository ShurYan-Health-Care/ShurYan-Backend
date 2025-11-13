using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Enums;

namespace Shuryan.Application.DTOs.Common.Address
{
    public class AddressResponse
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public Governorate Governorate { get; set; }
        public string? BuildingNumber { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}