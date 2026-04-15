namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyBasicInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public int VerificationStatus { get; set; }
        public string VerificationStatusName { get; set; } = string.Empty;
    }
}
