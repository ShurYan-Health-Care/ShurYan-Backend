namespace Shuryan.Application.DTOs.Responses.Patient
{
    public class NearbyPharmacyResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double DistanceInKm { get; set; }
        public bool OffersDelivery { get; set; }
        public decimal DeliveryFee { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
