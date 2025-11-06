using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Payment;
using Shuryan.Application.DTOs.Responses.Payment;

namespace Shuryan.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<ApiResponse<InitiatePaymentResponse>> InitiatePaymentAsync(Guid userId, InitiatePaymentRequest request, string? ipAddress = null);
        Task<ApiResponse<PaymentResponse>> ConfirmPaymentAsync(ConfirmPaymentRequest request);
        Task<ApiResponse<PaymentResponse>> CancelPaymentAsync(Guid paymentId, Guid userId);
        Task<ApiResponse<PaymentResponse>> RefundPaymentAsync(RefundPaymentRequest request, Guid adminUserId);
        Task<ApiResponse<PaymentResponse>> GetPaymentByIdAsync(Guid paymentId, Guid userId);
        Task<ApiResponse<IEnumerable<PaymentResponse>>> GetUserPaymentsAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<IEnumerable<PaymentResponse>>> GetOrderPaymentsAsync(string orderType, Guid orderId, Guid userId);
        Task<ApiResponse<PaymentResponse>> HandleProviderCallbackAsync(string provider, Dictionary<string, string> callbackData);
    }
}
