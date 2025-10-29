namespace Shuryan.Application.DTOs.Responses.Clinic
{
    public class ClinicAddressResponse
    {
        public string Governorate { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string BuildingNumber { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
