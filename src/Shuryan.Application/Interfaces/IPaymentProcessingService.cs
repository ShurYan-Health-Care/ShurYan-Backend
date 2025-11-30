using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Responses.Payment;
using Shuryan.Core.Enums.Payment;

namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Service for processing payments across different order types
    /// Handles business logic for Appointments, Pharmacy Orders, and Lab Orders
    /// </summary>
    public interface IPaymentProcessingService
    {
        /// <summary>
        /// Initiate payment for an appointment booking
        /// </summary>
        Task<ApiResponse<InitiatePaymentResponse>> InitiateAppointmentPaymentAsync(
            Guid userId,
            Guid appointmentId,
            PaymentMethod paymentMethod,
            PaymentType paymentType,
            string? ipAddress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Initiate payment for a pharmacy order
        /// </summary>
        Task<ApiResponse<InitiatePaymentResponse>> InitiatePharmacyOrderPaymentAsync(
            Guid userId,
            Guid pharmacyOrderId,
            PaymentMethod paymentMethod,
            PaymentType paymentType,
            string? ipAddress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Initiate payment for a lab order
        /// </summary>
        Task<ApiResponse<InitiatePaymentResponse>> InitiateLabOrderPaymentAsync(
            Guid userId,
            Guid labOrderId,
            PaymentMethod paymentMethod,
            PaymentType paymentType,
            string? ipAddress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Handle Paymob webhook callback
        /// </summary>
        Task<ApiResponse<PaymentResponse>> HandlePaymobWebhookAsync(
            string hmac,
            string webhookJson,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get payment details by ID
        /// </summary>
        Task<ApiResponse<PaymentResponse>> GetPaymentByIdAsync(
            Guid paymentId,
            Guid userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancel a pending payment
        /// </summary>
        Task<ApiResponse<PaymentResponse>> CancelPaymentAsync(
            Guid paymentId,
            Guid userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// [TEST ONLY] Simulate successful payment for testing purposes
        /// </summary>
        Task<ApiResponse<string>> SimulatePaymentSuccessAsync(
            Guid userId,
            string orderType,
            Guid orderId,
            CancellationToken cancellationToken = default);
    }
}
