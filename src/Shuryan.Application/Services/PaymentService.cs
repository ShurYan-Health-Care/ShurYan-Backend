using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Payment;
using Shuryan.Application.DTOs.Responses.Payment;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.External.Payments;
using Shuryan.Core.Enums.Payment;
using Shuryan.Core.Enums.Pharmacy;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<InitiatePaymentResponse>> InitiatePaymentAsync(
            Guid userId, 
            InitiatePaymentRequest request, 
            string? ipAddress = null)
        {
            try
            {
                // Validate order exists based on OrderType
                var orderExists = await ValidateOrderExistsAsync(request.OrderType, request.OrderId, userId);
                if (!orderExists)
                {
                    return ApiResponse<InitiatePaymentResponse>.Failure(
                        "الطلب غير موجود",
                        new[] { "Order not found or you don't have access to it" },
                        404);
                }

                // Create payment record
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    OrderType = request.OrderType,
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    Provider = request.Provider,
                    Status = PaymentStatus.Pending,
                    IpAddress = ipAddress,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Payments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                // Create initial transaction
                var transaction = new PaymentTransaction
                {
                    Id = Guid.NewGuid(),
                    PaymentId = payment.Id,
                    TransactionType = "Initiation",
                    Amount = request.Amount,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<PaymentTransaction>().AddAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Payment initiated: {PaymentId} for user {UserId}", payment.Id, userId);

                // Handle different payment methods
                var response = new InitiatePaymentResponse
                {
                    PaymentId = payment.Id,
                    RequiresRedirect = false,
                    Message = "تم إنشاء عملية الدفع بنجاح"
                };

                // For cash on delivery, no further action needed
                if (request.PaymentMethod == PaymentMethod.CashOnDelivery)
                {
                    response.Message = "سيتم الدفع عند الاستلام";
                    return ApiResponse<InitiatePaymentResponse>.Success(response, response.Message);
                }

                // For online payment methods, generate payment URL
                if (request.Provider.HasValue)
                {
                    response.RequiresRedirect = true;
                    response.PaymentUrl = GeneratePaymentUrl(payment, request.Provider.Value, request.ReturnUrl);
                    response.Message = "يرجى إكمال عملية الدفع من خلال الرابط المرفق";
                }

                return ApiResponse<InitiatePaymentResponse>.Success(response, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating payment for user {UserId}", userId);
                return ApiResponse<InitiatePaymentResponse>.Failure(
                    "حدث خطأ أثناء إنشاء عملية الدفع",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<PaymentResponse>> ConfirmPaymentAsync(ConfirmPaymentRequest request)
        {
            try
            {
                var payment = await _unitOfWork.Payments.GetPaymentWithTransactionsAsync(request.PaymentId);
                if (payment == null)
                {
                    return ApiResponse<PaymentResponse>.Failure(
                        "عملية الدفع غير موجودة",
                        new[] { "Payment not found" },
                        404);
                }

                if (payment.Status != PaymentStatus.Pending && payment.Status != PaymentStatus.Processing)
                {
                    return ApiResponse<PaymentResponse>.Failure(
                        "لا يمكن تأكيد هذه العملية",
                        new[] { $"Payment is already {payment.Status}" },
                        400);
                }

                // Update payment status
                payment.Status = PaymentStatus.Completed;
                payment.CompletedAt = DateTime.UtcNow;
                payment.ProviderTransactionId = request.ProviderTransactionId;
                payment.ProviderResponse = request.ProviderResponse;

                // Create confirmation transaction
                var transaction = new PaymentTransaction
                {
                    Id = Guid.NewGuid(),
                    PaymentId = payment.Id,
                    TransactionType = "Confirmation",
                    Amount = payment.Amount,
                    Status = PaymentStatus.Completed,
                    ProviderTransactionId = request.ProviderTransactionId,
                    ProviderResponse = request.ProviderResponse,
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<PaymentTransaction>().AddAsync(transaction);
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                // Update order status based on payment confirmation
                await UpdateOrderStatusAfterPaymentAsync(payment.OrderType, payment.OrderId);

                _logger.LogInformation("Payment confirmed: {PaymentId}", payment.Id);

                var response = _mapper.Map<PaymentResponse>(payment);
                return ApiResponse<PaymentResponse>.Success(response, "تم تأكيد الدفع بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment {PaymentId}", request.PaymentId);
                return ApiResponse<PaymentResponse>.Failure(
                    "حدث خطأ أثناء تأكيد الدفع",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<PaymentResponse>> CancelPaymentAsync(Guid paymentId, Guid userId)
        {
            try
            {
                var payment = await _unitOfWork.Payments.GetPaymentWithTransactionsAsync(paymentId);
                if (payment == null)
                {
                    return ApiResponse<PaymentResponse>.Failure(
                        "عملية الدفع غير موجودة",
                        new[] { "Payment not found" },
                        404);
                }

                if (payment.UserId != userId)
                {
                    return ApiResponse<PaymentResponse>.Failure(
                        "غير مصرح لك بإلغاء هذه العملية",
                        new[] { "Unauthorized" },
                        403);
                }

                if (payment.Status == PaymentStatus.Completed)
                {
                    return ApiResponse<PaymentResponse>.Failure(
                        "لا يمكن إلغاء عملية دفع مكتملة",
                        new[] { "Cannot cancel completed payment" },
                        400);
                }

                payment.Status = PaymentStatus.Cancelled;
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Payment cancelled: {PaymentId} by user {UserId}", paymentId, userId);

                var response = _mapper.Map<PaymentResponse>(payment);
                return ApiResponse<PaymentResponse>.Success(response, "تم إلغاء عملية الدفع");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling payment {PaymentId}", paymentId);
                return ApiResponse<PaymentResponse>.Failure(
                    "حدث خطأ أثناء إلغاء الدفع",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<PaymentResponse>> RefundPaymentAsync(RefundPaymentRequest request, Guid adminUserId)
        {
            try
            {
                var payment = await _unitOfWork.Payments.GetPaymentWithTransactionsAsync(request.PaymentId);
                if (payment == null)
                {
                    return ApiResponse<PaymentResponse>.Failure(
                        "عملية الدفع غير موجودة",
                        new[] { "Payment not found" },
                        404);
                }

                if (payment.Status != PaymentStatus.Completed)
                {
                    return ApiResponse<PaymentResponse>.Failure(
                        "يمكن استرجاع المبالغ المدفوعة فقط",
                        new[] { "Can only refund completed payments" },
                        400);
                }

                var maxRefundAmount = payment.Amount - (payment.RefundedAmount ?? 0);
                if (request.Amount > maxRefundAmount)
                {
                    return ApiResponse<PaymentResponse>.Failure(
                        "المبلغ المطلوب استرجاعه أكبر من المتاح",
                        new[] { $"Maximum refund amount is {maxRefundAmount}" },
                        400);
                }

                // Update payment
                payment.RefundedAmount = (payment.RefundedAmount ?? 0) + request.Amount;
                payment.RefundedAt = DateTime.UtcNow;
                payment.RefundReason = request.Reason;
                payment.Status = payment.RefundedAmount >= payment.Amount 
                    ? PaymentStatus.Refunded 
                    : PaymentStatus.PartiallyRefunded;

                // Create refund transaction
                var transaction = new PaymentTransaction
                {
                    Id = Guid.NewGuid(),
                    PaymentId = payment.Id,
                    TransactionType = "Refund",
                    Amount = request.Amount,
                    Status = PaymentStatus.Completed,
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow,
                    Metadata = $"Refund reason: {request.Reason}"
                };

                await _unitOfWork.Repository<PaymentTransaction>().AddAsync(transaction);
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Payment refunded: {PaymentId}, Amount: {Amount}", payment.Id, request.Amount);

                var response = _mapper.Map<PaymentResponse>(payment);
                return ApiResponse<PaymentResponse>.Success(response, "تم استرجاع المبلغ بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment {PaymentId}", request.PaymentId);
                return ApiResponse<PaymentResponse>.Failure(
                    "حدث خطأ أثناء استرجاع المبلغ",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<PaymentResponse>> GetPaymentByIdAsync(Guid paymentId, Guid userId)
        {
            try
            {
                var payment = await _unitOfWork.Payments.GetPaymentWithTransactionsAsync(paymentId);
                if (payment == null)
                {
                    return ApiResponse<PaymentResponse>.Failure(
                        "عملية الدفع غير موجودة",
                        new[] { "Payment not found" },
                        404);
                }

                if (payment.UserId != userId)
                {
                    return ApiResponse<PaymentResponse>.Failure(
                        "غير مصرح لك بعرض هذه العملية",
                        new[] { "Unauthorized" },
                        403);
                }

                var response = _mapper.Map<PaymentResponse>(payment);
                return ApiResponse<PaymentResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment {PaymentId}", paymentId);
                return ApiResponse<PaymentResponse>.Failure(
                    "حدث خطأ أثناء جلب بيانات الدفع",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<IEnumerable<PaymentResponse>>> GetUserPaymentsAsync(
            Guid userId, 
            int pageNumber = 1, 
            int pageSize = 10)
        {
            try
            {
                var payments = await _unitOfWork.Payments.GetPaymentsByUserIdAsync(userId, pageNumber, pageSize);
                var response = _mapper.Map<IEnumerable<PaymentResponse>>(payments);
                return ApiResponse<IEnumerable<PaymentResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for user {UserId}", userId);
                return ApiResponse<IEnumerable<PaymentResponse>>.Failure(
                    "حدث خطأ أثناء جلب عمليات الدفع",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<IEnumerable<PaymentResponse>>> GetOrderPaymentsAsync(
            string orderType, 
            Guid orderId, 
            Guid userId)
        {
            try
            {
                var payments = await _unitOfWork.Payments.GetPaymentsByOrderAsync(orderType, orderId);
                
                // Verify user has access to this order
                if (payments.Any() && payments.First().UserId != userId)
                {
                    return ApiResponse<IEnumerable<PaymentResponse>>.Failure(
                        "غير مصرح لك بعرض هذه العمليات",
                        new[] { "Unauthorized" },
                        403);
                }

                var response = _mapper.Map<IEnumerable<PaymentResponse>>(payments);
                return ApiResponse<IEnumerable<PaymentResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for order {OrderId}", orderId);
                return ApiResponse<IEnumerable<PaymentResponse>>.Failure(
                    "حدث خطأ أثناء جلب عمليات الدفع",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<PaymentResponse>> HandleProviderCallbackAsync(
            string provider, 
            Dictionary<string, string> callbackData)
        {
            try
            {
                // This is a placeholder - implement based on your payment provider
                _logger.LogInformation("Received callback from provider {Provider}", provider);
                
                
                return ApiResponse<PaymentResponse>.Failure(
                    "Provider callback handling not implemented",
                    new[] { "Not implemented" },
                    501);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling provider callback from {Provider}", provider);
                return ApiResponse<PaymentResponse>.Failure(
                    "حدث خطأ أثناء معالجة رد مزود الدفع",
                    new[] { ex.Message },
                    500);
            }
        }

        #region Private Helper Methods

        private async Task<bool> ValidateOrderExistsAsync(string orderType, Guid orderId, Guid userId)
        {
            return orderType switch
            {
                "PharmacyOrder" => await _unitOfWork.PharmacyOrders.GetByIdAsync(orderId) != null,
                "LabOrder" => await _unitOfWork.LabOrders.GetByIdAsync(orderId) != null,
                _ => false
            };
        }

        private async Task UpdateOrderStatusAfterPaymentAsync(string orderType, Guid orderId)
        {
            try
            {
                switch (orderType)
                {
                    case "PharmacyOrder":
                        var pharmacyOrder = await _unitOfWork.PharmacyOrders.GetByIdAsync(orderId);
                        if (pharmacyOrder != null && pharmacyOrder.Status == PharmacyOrderStatus.PendingPayment)
                        {
                            pharmacyOrder.Status = PharmacyOrderStatus.Confirmed;
                            pharmacyOrder.UpdatedAt = DateTime.UtcNow;
                            _unitOfWork.PharmacyOrders.Update(pharmacyOrder);
                            await _unitOfWork.SaveChangesAsync();
                        }
                        break;

                    case "LabOrder":
                        var labOrder = await _unitOfWork.LabOrders.GetByIdAsync(orderId);
                        if (labOrder != null && labOrder.Status == Core.Enums.Laboratory.LabOrderStatus.AwaitingPayment)
                        {
                            labOrder.Status = Core.Enums.Laboratory.LabOrderStatus.Paid;
                            labOrder.PaidAt = DateTime.UtcNow;
                            labOrder.UpdatedAt = DateTime.UtcNow;
                            _unitOfWork.LabOrders.Update(labOrder);
                            await _unitOfWork.SaveChangesAsync();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status after payment for {OrderType} {OrderId}", orderType, orderId);
            }
        }

        private string GeneratePaymentUrl(Payment payment, PaymentProvider provider, string? returnUrl)
        {
            // This is a placeholder - implement based on your payment provider
            var baseUrl = returnUrl ?? "https://shuryan.com/payment/callback";
            return $"{baseUrl}?paymentId={payment.Id}&provider={provider}";
        }

        #endregion
    }
}
