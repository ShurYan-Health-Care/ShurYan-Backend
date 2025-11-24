using System.Text.Json.Serialization;

namespace Shuryan.Application.DTOs.Paymob
{
    public class PaymobOrderRequest
    {
        [JsonPropertyName("auth_token")]
        public string AuthToken { get; set; } = string.Empty;

        [JsonPropertyName("delivery_needed")]
        public bool DeliveryNeeded { get; set; } = false;

        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "EGP";

        [JsonPropertyName("merchant_order_id")]
        public string MerchantOrderId { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<PaymobOrderItem> Items { get; set; } = new();
    }

    public class PaymobOrderItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 1;
    }
}
