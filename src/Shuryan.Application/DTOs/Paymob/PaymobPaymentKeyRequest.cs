using System.Text.Json.Serialization;

namespace Shuryan.Application.DTOs.Paymob
{
    public class PaymobPaymentKeyRequest
    {
        [JsonPropertyName("auth_token")]
        public string AuthToken { get; set; } = string.Empty;

        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        [JsonPropertyName("expiration")]
        public int Expiration { get; set; } = 3600; // 1 hour

        [JsonPropertyName("order_id")]
        public int OrderId { get; set; }

        [JsonPropertyName("billing_data")]
        public PaymobBillingData BillingData { get; set; } = new();

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "EGP";

        [JsonPropertyName("integration_id")]
        public int IntegrationId { get; set; }

        [JsonPropertyName("lock_order_when_paid")]
        public bool LockOrderWhenPaid { get; set; } = true;
    }

    public class PaymobBillingData
    {
        [JsonPropertyName("apartment")]
        public string Apartment { get; set; } = "NA";

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("floor")]
        public string Floor { get; set; } = "NA";

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("street")]
        public string Street { get; set; } = "NA";

        [JsonPropertyName("building")]
        public string Building { get; set; } = "NA";

        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("shipping_method")]
        public string ShippingMethod { get; set; } = "NA";

        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; } = "NA";

        [JsonPropertyName("city")]
        public string City { get; set; } = "NA";

        [JsonPropertyName("country")]
        public string Country { get; set; } = "EG";

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        public string State { get; set; } = "NA";
    }
}
