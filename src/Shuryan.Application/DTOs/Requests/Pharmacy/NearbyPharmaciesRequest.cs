using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class NearbyPharmaciesRequest
    {
        [Required(ErrorMessage = "Latitude is required")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double Longitude { get; set; }

        [Range(1, 100, ErrorMessage = "Radius must be between 1 and 100 km")]
        public double RadiusInKm { get; set; } = 10;

        [Range(1, 100)]
        public int MaxResults { get; set; } = 20;
    }
}
