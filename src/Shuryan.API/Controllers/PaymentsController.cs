//using System;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Shuryan.Application.DTOs.Requests.Payment;
//using Shuryan.Application.Interfaces;

//namespace Shuryan.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize]
//    public class PaymentsController : ControllerBase
//    {
//        private readonly IPaymentService _paymentService;

//        public PaymentsController(IPaymentService paymentService)
//        {
//            _paymentService = paymentService;
//        }

//        /// <summary>
//        /// بدء عملية دفع جديدة
//        /// </summary>
//        [HttpPost("initiate")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request)
//        {
//            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
//            var result = await _paymentService.InitiatePaymentAsync(userId, request, ipAddress);
//            return StatusCode(result.StatusCode ?? 500, result);
//        }

//        /// <summary>
//        /// تأكيد عملية الدفع
//        /// </summary>
//        [HttpPost("confirm")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
//        {
//            var result = await _paymentService.ConfirmPaymentAsync(request);
//            return StatusCode(result.StatusCode ?? 500, result);
//        }

//        /// <summary>
//        /// إلغاء عملية دفع
//        /// </summary>
//        [HttpPost("{paymentId}/cancel")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        public async Task<IActionResult> CancelPayment(Guid paymentId)
//        {
//            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//            var result = await _paymentService.CancelPaymentAsync(paymentId, userId);
//            return StatusCode(result.StatusCode ?? 500, result);
//        }

//        /// <summary>
//        /// استرجاع مبلغ (للإدارة فقط)
//        /// </summary>
//        [HttpPost("refund")]
//        [Authorize(Roles = "Admin")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        public async Task<IActionResult> RefundPayment([FromBody] RefundPaymentRequest request)
//        {
//            var adminUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//            var result = await _paymentService.RefundPaymentAsync(request, adminUserId);
//            return StatusCode(result.StatusCode ?? 500, result);
//        }

//        /// <summary>
//        /// الحصول على تفاصيل عملية دفع
//        /// </summary>
//        [HttpGet("{paymentId}")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status403Forbidden)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        public async Task<IActionResult> GetPaymentById(Guid paymentId)
//        {
//            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//            var result = await _paymentService.GetPaymentByIdAsync(paymentId, userId);
//            return StatusCode(result.StatusCode ?? 500, result);
//        }

//        /// <summary>
//        /// الحصول على عمليات الدفع الخاصة بالمستخدم
//        /// </summary>
//        [HttpGet("my-payments")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        public async Task<IActionResult> GetMyPayments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
//        {
//            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//            var result = await _paymentService.GetUserPaymentsAsync(userId, pageNumber, pageSize);
//            return StatusCode(result.StatusCode ?? 500, result);
//        }

//        /// <summary>
//        /// الحصول على عمليات الدفع الخاصة بطلب معين
//        /// </summary>
//        [HttpGet("order/{orderType}/{orderId}")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status403Forbidden)]
//        public async Task<IActionResult> GetOrderPayments(string orderType, Guid orderId)
//        {
//            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//            var result = await _paymentService.GetOrderPaymentsAsync(orderType, orderId, userId);
//            return StatusCode(result.StatusCode ?? 500, result);
//        }

//        /// <summary>
//        /// Webhook لاستقبال ردود مزودي الدفع
//        /// </summary>
//        [HttpPost("callback/{provider}")]
//        [AllowAnonymous]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        public async Task<IActionResult> PaymentCallback(string provider)
//        {
//            // Extract callback data from request
//            var callbackData = new System.Collections.Generic.Dictionary<string, string>();
            
//            foreach (var key in Request.Form.Keys)
//            {
//                callbackData[key] = Request.Form[key].ToString();
//            }

//            foreach (var key in Request.Query.Keys)
//            {
//                callbackData[key] = Request.Query[key].ToString();
//            }

//            var result = await _paymentService.HandleProviderCallbackAsync(provider, callbackData);
//            return StatusCode(result.StatusCode ?? 500, result);
//        }
//    }
//}
