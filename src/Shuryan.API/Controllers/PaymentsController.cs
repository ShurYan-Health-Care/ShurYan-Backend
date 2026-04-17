using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PaymobWebhook(CancellationToken cancellationToken)
        {
            // Get HMAC from query string
            var hmac = Request.Query["hmac"].ToString();
            
            // Read webhook body
            using var reader = new StreamReader(Request.Body);
            var webhookJson = await reader.ReadToEndAsync(cancellationToken);

            var result = await _paymentProcessingService.HandlePaymobWebhookAsync(
                hmac,
                webhookJson,
                cancellationToken);

            return StatusCode(result.StatusCode ?? 500, result);
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

        /// <summary>
        /// [TEST ONLY] محاكاة نجاح الدفع لطلب معمل - للتجربة فقط
        /// </summary>
        [HttpPost("lab-orders/{labOrderId}/simulate-payment-success")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SimulateLabOrderPaymentSuccess(
            Guid labOrderId,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _paymentProcessingService.SimulatePaymentSuccessAsync(
                userId,
                "LabOrder",
                labOrderId,
                cancellationToken);

            return StatusCode(result.StatusCode ?? 500, result);
        }

        /// <summary>
        /// اختبار يدوي للـ webhook (Development only)
        /// </summary>
        [HttpPost("{paymentId}/test-success")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> TestPaymentSuccess(
            Guid paymentId,
            CancellationToken cancellationToken)
        {
            var webhookJson = $$"""
            {
              "obj": {
                "id": 123456789,
                "success": true,
                "pending": false,
                "amount_cents": 10000,
                "currency": "EGP",
                "error_occured": false,
                "has_parent_transaction": false,
                "order": {
                  "id": 987654321,
                  "merchant_order_id": "{{paymentId}}"
                },
                "created_at": "2025-11-21T01:00:00.000000Z",
                "transaction_processed_callback_responses": []
              },
              "type": "TRANSACTION"
            }
            """;

            var result = await _paymentProcessingService.HandlePaymobWebhookAsync(
                "TEST_HMAC",
                webhookJson,
                cancellationToken);

            return StatusCode(result.StatusCode ?? 500, result);
        }
    }
}
