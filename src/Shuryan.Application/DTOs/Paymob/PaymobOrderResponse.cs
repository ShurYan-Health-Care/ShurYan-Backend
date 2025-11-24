using System.Text.Json.Serialization;

namespace Shuryan.Application.DTOs.Paymob
{
    public class PaymobOrderResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("delivery_needed")]
        public bool DeliveryNeeded { get; set; }

        [JsonPropertyName("merchant")]
        public PaymobMerchant? Merchant { get; set; }

        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        [JsonPropertyName("shipping_data")]
        public object? ShippingData { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("is_payment_locked")]
        public bool IsPaymentLocked { get; set; }

        [JsonPropertyName("merchant_order_id")]
        public string MerchantOrderId { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<object>? Items { get; set; }
    }

    public class PaymobMerchant
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; } = string.Empty;
    }
}
