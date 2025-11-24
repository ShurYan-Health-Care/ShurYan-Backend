using System.Text.Json.Serialization;

namespace Shuryan.Application.DTOs.Paymob
{
    public class PaymobPaymentKeyResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }
}
