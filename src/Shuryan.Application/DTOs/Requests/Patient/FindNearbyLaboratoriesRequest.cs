namespace Shuryan.Application.DTOs.Requests.Patient
{
    /// <summary>
    /// Request للبحث عن أقرب 3 معامل
    /// </summary>
    public class FindNearbyLaboratoriesRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
