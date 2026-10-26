using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Shuryan.Application.DTOs.Requests.Payment;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Settings;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentProcessingService _paymentProcessingService;
        private readonly FrontendSettings _frontendSettings;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentProcessingService paymentProcessingService,
            IOptions<FrontendSettings> frontendSettings,
            ILogger<PaymentsController> logger)
        {
            _paymentProcessingService = paymentProcessingService;
            _frontendSettings = frontendSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// بدء عملية دفع لحجز موعد مع دكتور
        /// </summary>
        [HttpPost("appointments/{appointmentId}/initiate")]
        [EnableRateLimiting("payment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InitiateAppointmentPayment(
            Guid appointmentId,
            [FromBody] InitiateAppointmentPaymentRequest request,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var result = await _paymentProcessingService.InitiateAppointmentPaymentAsync(
                userId,
                appointmentId,
                request.PaymentMethod,
                request.PaymentType,
                ipAddress,
                cancellationToken);

            return StatusCode(result.StatusCode ?? 500, result);
        }

        /// <summary>
        /// بدء عملية دفع لطلب صيدلية
        /// </summary>
        [HttpPost("pharmacy-orders/{pharmacyOrderId}/initiate")]
        [EnableRateLimiting("payment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InitiatePharmacyOrderPayment(
            Guid pharmacyOrderId,
            [FromBody] InitiatePharmacyOrderPaymentRequest request,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var result = await _paymentProcessingService.InitiatePharmacyOrderPaymentAsync(
                userId,
                pharmacyOrderId,
                request.PaymentMethod,
                request.PaymentType,
                ipAddress,
                cancellationToken);

            return StatusCode(result.StatusCode ?? 500, result);
        }

        /// <summary>
        /// بدء عملية دفع لطلب معمل
        /// </summary>
        [HttpPost("lab-orders/{labOrderId}/initiate")]
        [EnableRateLimiting("payment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InitiateLabOrderPayment(
            Guid labOrderId,
            [FromBody] InitiateLabOrderPaymentRequest request,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var result = await _paymentProcessingService.InitiateLabOrderPaymentAsync(
                userId,
                labOrderId,
                request.PaymentMethod,
                request.PaymentType,
                ipAddress,
                cancellationToken);

            return StatusCode(result.StatusCode ?? 500, result);
        }

        /// <summary>
        /// Paymob Transaction Response Callback - إعادة توجيه المتصفح بعد الدفع
        /// Also acts as a safety-net: if the POST webhook failed to arrive or was rejected,
        /// this endpoint will still confirm the payment before redirecting the user.
        /// </summary>
        [HttpGet("response/paymob")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public async Task<IActionResult> PaymobResponseCallback(CancellationToken cancellationToken)
        {
            var isSuccess = Request.Query["success"].ToString().ToLower() == "true";
            var merchantOrderId = Request.Query["merchant_order_id"].ToString();

            _logger.LogInformation(
                "[Callback] Received. success={Success}, merchant_order_id={MerchantOrderId}",
                isSuccess, merchantOrderId);

            // Safety-net: confirm the payment via the GET callback in case the webhook failed.
            // ProcessCallbackPaymentAsync is fully idempotent — skips if already Completed.
            if (isSuccess && Guid.TryParse(merchantOrderId, out var paymentId))
            {
                try
                {
                    await _paymentProcessingService.ProcessCallbackPaymentAsync(paymentId, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Non-fatal — we still redirect the user. Log for investigation.
                    _logger.LogError(ex,
                        "[Callback] ProcessCallbackPaymentAsync failed for payment {PaymentId}", paymentId);
                }
            }

            var path = isSuccess
                ? _frontendSettings.PaymentSuccessUrl
                : _frontendSettings.PaymentFailedUrl;

            // Forward all Paymob query params to the frontend page as-is
            var queryString = Request.QueryString.Value ?? string.Empty;
            var redirectUrl = $"{_frontendSettings.BaseUrl}{path}{queryString}";

            return Redirect(redirectUrl);
        }

        /// <summary>
        /// Paymob Webhook - استقبال تحديثات حالة الدفع
        /// </summary>
        [HttpPost("webhook/paymob")]
        [AllowAnonymous]
        [EnableRateLimiting("webhook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PaymobWebhook(CancellationToken cancellationToken)
        {
            var webhookJson = string.Empty;
            try
            {
                // Get HMAC from query string
                var hmac = Request.Query["hmac"].ToString();

                // Read webhook body
                using var reader = new StreamReader(Request.Body);
                webhookJson = await reader.ReadToEndAsync(cancellationToken);

                _logger.LogInformation(
                    "[Webhook] Received. HMAC={Hmac}, BodyLength={Length}",
                    hmac, webhookJson.Length);

                var result = await _paymentProcessingService.HandlePaymobWebhookAsync(
                    hmac,
                    webhookJson,
                    cancellationToken);

                _logger.LogInformation(
                    "[Webhook] Processed. StatusCode={StatusCode}, Success={Success}",
                    result.StatusCode, result.IsSuccess);

                // Always return 200 to Paymob to prevent retries
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log full exception so it surfaces in MonsterASP / IIS logs
                _logger.LogError(ex,
                    "[Webhook] Unhandled exception. Body={Body}", webhookJson);

                // Returning non-200 causes Paymob to keep retrying — always return 200
                return Ok(new { status = "received", error = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على تفاصيل عملية دفع
        /// </summary>
        [HttpGet("{paymentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPaymentById(
            Guid paymentId,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _paymentProcessingService.GetPaymentByIdAsync(
                paymentId,
                userId,
                cancellationToken);

            return StatusCode(result.StatusCode ?? 500, result);
        }

        /// <summary>
        /// إلغاء عملية دفع معلقة
        /// </summary>
        [HttpPost("{paymentId}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelPayment(
            Guid paymentId,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _paymentProcessingService.CancelPaymentAsync(
                paymentId,
                userId,
                cancellationToken);

            return StatusCode(result.StatusCode ?? 500, result);
        }


    }
}
