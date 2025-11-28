namespace Shuryan.Application.DTOs.Responses.Patient
{
    /// <summary>
    /// Response للبحث عن أقرب 3 معامل من المريض
    /// </summary>
    public class FindNearbyLaboratoriesResponse
    {
        public List<NearbyLaboratorySimpleResponse> NearbyLaboratories { get; set; } = new();
        public int TotalFound { get; set; }
        public double SearchRadiusKm { get; set; }
    }
}
