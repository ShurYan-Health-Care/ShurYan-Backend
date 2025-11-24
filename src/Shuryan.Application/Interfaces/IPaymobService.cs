using Shuryan.Application.DTOs.Paymob;

namespace Shuryan.Application.Interfaces
{
    public interface IPaymobService
    {
        /// <summary>
        /// Authenticate with Paymob and get auth token
        /// </summary>
        Task<string> AuthenticateAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Create order in Paymob
        /// </summary>
        Task<PaymobOrderResponse> CreateOrderAsync(
            string authToken,
            decimal amount,
            string merchantOrderId,
            string itemName,
            string itemDescription,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generate payment key for iframe/mobile wallet
        /// </summary>
        Task<string> GeneratePaymentKeyAsync(
            string authToken,
            int paymobOrderId,
            decimal amount,
            string integrationId,
            string userEmail,
            string userFirstName,
            string userLastName,
            string userPhone,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify webhook HMAC signature
        /// </summary>
        bool VerifyWebhookSignature(string receivedHmac, PaymobWebhookRequest webhookData);

        /// <summary>
        /// Get payment iframe URL
        /// </summary>
        string GetIFrameUrl(string paymentToken, int iframeId);
    }
}
