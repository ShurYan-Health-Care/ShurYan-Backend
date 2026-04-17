using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuryan.Application.DTOs.Paymob;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Settings;

namespace Shuryan.Application.Services
{
    public class PaymobService : IPaymobService
    {
        private readonly HttpClient _httpClient;
        private readonly PaymobSettings _settings;
        private readonly ILogger<PaymobService> _logger;

        public PaymobService(
            HttpClient httpClient,
            IOptions<PaymobSettings> settings,
            ILogger<PaymobService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        }

        public async Task<string> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new PaymobAuthRequest { ApiKey = _settings.APIKey };

                var response = await _httpClient.PostAsJsonAsync(
                    "/api/auth/tokens",
                    request,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<PaymobAuthResponse>(cancellationToken: cancellationToken);

                if (result == null || string.IsNullOrEmpty(result.Token))
                {
                    _logger.LogError("Failed to authenticate with Paymob: Empty token received");
                    throw new InvalidOperationException("Failed to authenticate with Paymob");
                }

                _logger.LogInformation("Successfully authenticated with Paymob");
                return result.Token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating with Paymob");
                throw;
            }
        }

        public async Task<PaymobOrderResponse> CreateOrderAsync(
            string authToken,
            decimal amount,
            string merchantOrderId,
            string itemName,
            string itemDescription,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var amountCents = (int)(amount * 100);

                var request = new PaymobOrderRequest
                {
                    AuthToken = authToken,
                    DeliveryNeeded = false,
                    AmountCents = amountCents,
                    Currency = "EGP",
                    MerchantOrderId = merchantOrderId,
                    Items = new List<PaymobOrderItem>
                    {
                        new PaymobOrderItem
                        {
                            Name = itemName,
                            AmountCents = amountCents,
                            Description = itemDescription,
                            Quantity = 1
                        }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "/api/ecommerce/orders",
                    request,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<PaymobOrderResponse>(cancellationToken: cancellationToken);

                if (result == null)
                {
                    _logger.LogError("Failed to create Paymob order: Empty response");
                    throw new InvalidOperationException("Failed to create Paymob order");
                }

                _logger.LogInformation("Successfully created Paymob order {OrderId} for merchant order {MerchantOrderId}",
                    result.Id, merchantOrderId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Paymob order for merchant order {MerchantOrderId}", merchantOrderId);
                throw;
            }
        }

        public async Task<string> GeneratePaymentKeyAsync(
            string authToken,
            int paymobOrderId,
            decimal amount,
            string integrationId,
            string userEmail,
            string userFirstName,
            string userLastName,
            string userPhone,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var amountCents = (int)(amount * 100);

                var request = new PaymobPaymentKeyRequest
                {
                    AuthToken = authToken,
                    AmountCents = amountCents,
                    Expiration = 3600,
                    OrderId = paymobOrderId,
                    Currency = "EGP",
                    IntegrationId = int.Parse(integrationId),
                    LockOrderWhenPaid = true,
                    BillingData = new PaymobBillingData
                    {
                        Email = userEmail,
                        FirstName = userFirstName,
                        LastName = userLastName,
                        PhoneNumber = userPhone,
                        Apartment = "NA",
                        Floor = "NA",
                        Street = "NA",
                        Building = "NA",
                        ShippingMethod = "NA",
                        PostalCode = "NA",
                        City = "NA",
                        Country = "EG",
                        State = "NA"
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "/api/acceptance/payment_keys",
                    request,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<PaymobPaymentKeyResponse>(cancellationToken: cancellationToken);

                if (result == null || string.IsNullOrEmpty(result.Token))
                {
                    _logger.LogError("Failed to generate payment key: Empty token received");
                    throw new InvalidOperationException("Failed to generate payment key");
                }

                _logger.LogInformation("Successfully generated payment key for order {OrderId}", paymobOrderId);
                return result.Token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payment key for order {OrderId}", paymobOrderId);
                throw;
            }
        }

        public bool VerifyWebhookSignature(string receivedHmac, PaymobWebhookRequest webhookData)
        {
            try
            {
                if (webhookData?.Obj == null)
                {
                    _logger.LogWarning("Webhook data is null or missing transaction object");
                    return false;
                }

                var obj = webhookData.Obj;

                // Build the concatenated string as per Paymob documentation
                var concatenatedString = string.Concat(
                    obj.AmountCents,
                    obj.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ"),
                    obj.Currency,
                    obj.ErrorOccured.ToString().ToLower(),
                    obj.HasParentTransaction.ToString().ToLower(),
                    obj.Id,
                    obj.IntegrationId,
                    obj.IsAuth.ToString().ToLower(),
                    obj.IsCapture.ToString().ToLower(),
                    obj.IsRefunded.ToString().ToLower(),
                    obj.IsStandalonePayment.ToString().ToLower(),
                    obj.IsVoided.ToString().ToLower(),
                    obj.Order?.Id ?? 0,
                    obj.Owner,
                    obj.Pending.ToString().ToLower(),
                    obj.SourceData?.Pan ?? "NA",
                    obj.SourceData?.SubType ?? "NA",
                    obj.SourceData?.Type ?? "NA",
                    obj.Success.ToString().ToLower()
                );

                // Calculate HMAC
                using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_settings.HMAC));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenatedString));
                var calculatedHmac = BitConverter.ToString(hash).Replace("-", "").ToLower();

                var isValid = calculatedHmac == receivedHmac.ToLower();

                if (!isValid)
                {
                    _logger.LogWarning("HMAC verification failed. Received: {ReceivedHmac}, Calculated: {CalculatedHmac}",
                        receivedHmac, calculatedHmac);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying webhook HMAC");
                return false;
            }
        }

        public string GetIFrameUrl(string paymentToken, int iframeId)
        {
            return $"{_settings.IFrameUrl}/{iframeId}?payment_token={paymentToken}";
        }
    }
}
