using Shuryan.Core.Enums;
using System;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    /// <summary>
    /// Simplified address response for laboratory - without audit fields
    /// </summary>
    public class LaboratoryAddressResponse
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public string? BuildingNumber { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
