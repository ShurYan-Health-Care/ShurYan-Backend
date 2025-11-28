namespace Shuryan.Application.DTOs.Responses.Patient
{
    /// <summary>
    /// معمل قريب من المريض (Simple Version - فقط المعلومات الأساسية)
    /// </summary>
    public class NearbyLaboratorySimpleResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double DistanceInKm { get; set; }
        public bool OffersHomeSampleCollection { get; set; }
        public decimal? HomeSampleCollectionFee { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
