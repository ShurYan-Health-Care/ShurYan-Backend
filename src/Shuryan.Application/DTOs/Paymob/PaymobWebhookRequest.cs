using System.Text.Json.Serialization;

namespace Shuryan.Application.DTOs.Paymob
{
    public class PaymobWebhookRequest
    {
        [JsonPropertyName("obj")]
        public PaymobTransactionData? Obj { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    public class PaymobTransactionData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("pending")]
        public bool Pending { get; set; }

        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("is_auth")]
        public bool IsAuth { get; set; }

        [JsonPropertyName("is_capture")]
        public bool IsCapture { get; set; }

        [JsonPropertyName("is_standalone_payment")]
        public bool IsStandalonePayment { get; set; }

        [JsonPropertyName("is_voided")]
        public bool IsVoided { get; set; }

        [JsonPropertyName("is_refunded")]
        public bool IsRefunded { get; set; }

        [JsonPropertyName("is_3d_secure")]
        public bool Is3dSecure { get; set; }

        [JsonPropertyName("integration_id")]
        public int IntegrationId { get; set; }

        [JsonPropertyName("profile_id")]
        public int ProfileId { get; set; }

        [JsonPropertyName("has_parent_transaction")]
        public bool HasParentTransaction { get; set; }

        [JsonPropertyName("order")]
        public PaymobWebhookOrder? Order { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("source_data")]
        public PaymobSourceData? SourceData { get; set; }

        [JsonPropertyName("api_source")]
        public string ApiSource { get; set; } = string.Empty;

        [JsonPropertyName("terminal_id")]
        public object? TerminalId { get; set; }

        [JsonPropertyName("merchant_commission")]
        public int MerchantCommission { get; set; }

        [JsonPropertyName("installment")]
        public object? Installment { get; set; }

        [JsonPropertyName("discount_details")]
        public List<object>? DiscountDetails { get; set; }

        [JsonPropertyName("is_void")]
        public bool IsVoid { get; set; }

        [JsonPropertyName("is_refund")]
        public bool IsRefund { get; set; }

        [JsonPropertyName("data")]
        public PaymobTransactionMetadata? Data { get; set; }

        [JsonPropertyName("is_hidden")]
        public bool IsHidden { get; set; }

        [JsonPropertyName("payment_key_claims")]
        public PaymobPaymentKeyClaims? PaymentKeyClaims { get; set; }

        [JsonPropertyName("error_occured")]
        public bool ErrorOccured { get; set; }

        [JsonPropertyName("is_live")]
        public bool IsLive { get; set; }

        [JsonPropertyName("other_endpoint_reference")]
        public object? OtherEndpointReference { get; set; }

        [JsonPropertyName("refunded_amount_cents")]
        public int RefundedAmountCents { get; set; }

        [JsonPropertyName("source_id")]
        public int SourceId { get; set; }

        [JsonPropertyName("is_captured")]
        public bool IsCaptured { get; set; }

        [JsonPropertyName("captured_amount")]
        public int CapturedAmount { get; set; }

        [JsonPropertyName("merchant_staff_tag")]
        public object? MerchantStaffTag { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("is_settled")]
        public bool IsSettled { get; set; }

        [JsonPropertyName("bill_balanced")]
        public bool BillBalanced { get; set; }

        [JsonPropertyName("is_bill")]
        public bool IsBill { get; set; }

        [JsonPropertyName("owner")]
        public int Owner { get; set; }

        [JsonPropertyName("parent_transaction")]
        public object? ParentTransaction { get; set; }
    }

    public class PaymobWebhookOrder
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("delivery_needed")]
        public bool DeliveryNeeded { get; set; }

        [JsonPropertyName("merchant")]
        public PaymobWebhookMerchant? Merchant { get; set; }

        [JsonPropertyName("collector")]
        public object? Collector { get; set; }

        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        [JsonPropertyName("shipping_data")]
        public object? ShippingData { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("is_payment_locked")]
        public bool IsPaymentLocked { get; set; }

        [JsonPropertyName("is_return")]
        public bool IsReturn { get; set; }

        [JsonPropertyName("is_cancel")]
        public bool IsCancel { get; set; }

        [JsonPropertyName("is_returned")]
        public bool IsReturned { get; set; }

        [JsonPropertyName("is_canceled")]
        public bool IsCanceled { get; set; }

        [JsonPropertyName("merchant_order_id")]
        public string MerchantOrderId { get; set; } = string.Empty;

        [JsonPropertyName("wallet_notification")]
        public object? WalletNotification { get; set; }

        [JsonPropertyName("paid_amount_cents")]
        public int PaidAmountCents { get; set; }

        [JsonPropertyName("notify_user_with_email")]
        public bool NotifyUserWithEmail { get; set; }

        [JsonPropertyName("items")]
        public List<object>? Items { get; set; }

        [JsonPropertyName("order_url")]
        public string OrderUrl { get; set; } = string.Empty;

        [JsonPropertyName("commission_fees")]
        public int CommissionFees { get; set; }

        [JsonPropertyName("delivery_fees_cents")]
        public int DeliveryFeesCents { get; set; }

        [JsonPropertyName("delivery_vat_cents")]
        public int DeliveryVatCents { get; set; }

        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [JsonPropertyName("merchant_staff_tag")]
        public object? MerchantStaffTag { get; set; }

        [JsonPropertyName("api_source")]
        public string ApiSource { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public object? Data { get; set; }
    }

    public class PaymobWebhookMerchant
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("phones")]
        public List<string>? Phones { get; set; }

        [JsonPropertyName("company_emails")]
        public List<string>? CompanyEmails { get; set; }

        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; } = string.Empty;

        [JsonPropertyName("street")]
        public string Street { get; set; } = string.Empty;
    }

    public class PaymobSourceData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("pan")]
        public string Pan { get; set; } = string.Empty;

        [JsonPropertyName("sub_type")]
        public string SubType { get; set; } = string.Empty;
    }

    public class PaymobTransactionMetadata
    {
        [JsonPropertyName("klass")]
        public string Klass { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("gateway_integration_pk")]
        public int GatewayIntegrationPk { get; set; }
    }

    public class PaymobPaymentKeyClaims
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("integration_id")]
        public int IntegrationId { get; set; }

        [JsonPropertyName("order_id")]
        public int OrderId { get; set; }

        [JsonPropertyName("billing_data")]
        public PaymobBillingData? BillingData { get; set; }

        [JsonPropertyName("lock_order_when_paid")]
        public bool LockOrderWhenPaid { get; set; }

        [JsonPropertyName("exp")]
        public int Exp { get; set; }
    }
}
