namespace Shuryan.Application.DTOs.Responses.Auth
{
    public class GoogleAuthResponse
    {
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public AuthResponseDto? Data { get; set; }
    }
}
