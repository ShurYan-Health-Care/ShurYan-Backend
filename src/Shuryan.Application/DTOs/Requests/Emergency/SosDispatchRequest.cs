using System;

namespace Shuryan.Application.DTOs.Requests.Emergency
{
    public class SosDispatchRequest
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
