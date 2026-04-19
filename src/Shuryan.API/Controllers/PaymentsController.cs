using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shuryan.Application.DTOs.Requests.Payment;
using Shuryan.Application.Interfaces;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentProcessingService _paymentProcessingService;

        public PaymentsController(IPaymentProcessingService paymentProcessingService)
        {
            _paymentProcessingService = paymentProcessingService;
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

                var result = await _paymentProcessingService.HandlePaymobWebhookAsync(
                    hmac,
                    webhookJson,
                    cancellationToken);

                // Always return 200 to Paymob to prevent retries
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the error but ALWAYS return 200 to Paymob
                // Returning non-200 causes Paymob to keep retrying the webhook
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
