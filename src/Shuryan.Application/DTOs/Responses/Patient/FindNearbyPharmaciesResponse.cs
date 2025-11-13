namespace Shuryan.Application.DTOs.Responses.Patient
{
    public class FindNearbyPharmaciesResponse
    {
        public List<NearbyPharmacyResponse> NearbyPharmacies { get; set; } = new();
        public int TotalFound { get; set; }
        public double SearchRadiusKm { get; set; }
    }
}
