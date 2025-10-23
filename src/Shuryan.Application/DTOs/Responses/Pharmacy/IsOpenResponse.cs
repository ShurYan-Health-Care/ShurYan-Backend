namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class IsOpenResponse
    {
        public Guid PharmacyId { get; set; }
        public bool IsOpen { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CurrentTime { get; set; }
        public TimeSpan? OpeningTime { get; set; }
        public TimeSpan? ClosingTime { get; set; }
    }
}
