using System.Text.Json.Serialization;

namespace Shuryan.Application.DTOs.Paymob
{
    public class PaymobAuthRequest
    {
        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; } = string.Empty;
    }
}
