using Microsoft.Extensions.Logging;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.External.Payments;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Enums.Laboratory;
using Shuryan.Core.Enums.Payment;
using Shuryan.Core.Enums.Pharmacy;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    /// <summary>
    /// Hangfire recurring job that runs every 30 minutes.
    /// Finds all payments in Pending status older than 1 hour and expires them,
    /// reverting the related order status so the patient can retry payment.
    /// </summary>
    public class PaymentExpiryJob : IPaymentExpiryJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationHubService _notificationHubService;
        private readonly ILogger<PaymentExpiryJob> _logger;

        // Payment is considered expired if it has been Pending for more than this duration
        private static readonly TimeSpan ExpiryThreshold = TimeSpan.FromHours(1);

        public PaymentExpiryJob(
            IUnitOfWork unitOfWork,
            INotificationHubService notificationHubService,
            ILogger<PaymentExpiryJob> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationHubService = notificationHubService;
            _logger = logger;
        }

        public async Task ExpirePendingPaymentsAsync()
        {
            _logger.LogInformation("PaymentExpiryJob started - checking for stale pending payments");

            try
            {
                var cutoffTime = DateTime.UtcNow.Subtract(ExpiryThreshold);

                // Get all pending payments older than the threshold
                var pendingPayments = await _unitOfWork.Payments
                    .GetPaymentsByStatusAsync(PaymentStatus.Pending, pageNumber: 1, pageSize: 100);

                var stalePayments = pendingPayments
                    .Where(p => p.CreatedAt < cutoffTime)
                    .ToList();

                if (!stalePayments.Any())
                {
                    _logger.LogInformation("PaymentExpiryJob: No stale pending payments found");
                    return;
                }

                _logger.LogInformation("PaymentExpiryJob: Found {Count} stale pending payments to expire", stalePayments.Count);

                foreach (var payment in stalePayments)
                {
                    try
                    {
                        await ExpireSinglePaymentAsync(payment);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "PaymentExpiryJob: Failed to expire payment {PaymentId}", payment.Id);
                        // Continue processing other payments
                    }
                }

                _logger.LogInformation("PaymentExpiryJob completed - expired {Count} payments", stalePayments.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PaymentExpiryJob failed with unhandled exception");
                throw;
            }
        }

        private async Task ExpireSinglePaymentAsync(Payment payment)
        {
            // Update payment status to Cancelled (expired)
            payment.Status = PaymentStatus.Cancelled;
            payment.FailedAt = DateTime.UtcNow;
            payment.FailureReason = "Payment expired - exceeded 1 hour pending limit";
            _unitOfWork.Payments.Update(payment);

            // Create expiry transaction record
            var transaction = new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                TransactionType = "Expired",
                Amount = payment.Amount,
                Status = PaymentStatus.Cancelled,
                ErrorMessage = "Payment expired automatically after 1 hour",
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow,
                Metadata = $"Auto-expired by PaymentExpiryJob at {DateTime.UtcNow:O}"
            };
            await _unitOfWork.Repository<PaymentTransaction>().AddAsync(transaction);

            // Revert related order status so patient can retry
            await RevertOrderStatusAsync(payment.OrderType, payment.OrderId);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Payment {PaymentId} expired (was pending since {CreatedAt}). OrderType: {OrderType}, OrderId: {OrderId}",
                payment.Id, payment.CreatedAt, payment.OrderType, payment.OrderId);

            // Send real-time notification to patient
            try
            {
                await _notificationHubService.SendNotificationToUserAsync(
                    payment.UserId,
                    "انتهت صلاحية الدفع",
                    "انتهت صلاحية عملية الدفع الخاصة بك. يمكنك المحاولة مرة أخرى",
                    new { paymentId = payment.Id, status = "Expired", orderType = payment.OrderType, orderId = payment.OrderId });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send PaymentExpired SignalR notification for payment {PaymentId}", payment.Id);
            }
        }

        private async Task RevertOrderStatusAsync(string orderType, Guid orderId)
        {
            switch (orderType)
            {
                case "ConsultationBooking":
                    var appointment = await _unitOfWork.Appointments.GetByIdAsync(orderId);
                    if (appointment != null && appointment.Status == AppointmentStatus.Confirmed)
                    {
                        appointment.Status = AppointmentStatus.PendingPayment;
                        appointment.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.Appointments.Update(appointment);
                        _logger.LogInformation("Appointment {AppointmentId} reverted to PendingPayment after payment expiry", orderId);
                    }
                    break;

                case "PharmacyOrder":
                    var pharmacyOrder = await _unitOfWork.PharmacyOrders.GetByIdAsync(orderId);
                    if (pharmacyOrder != null && pharmacyOrder.Status == PharmacyOrderStatus.PendingPayment)
                    {
                        // PharmacyOrder stays in PendingPayment — patient can retry
                        _logger.LogInformation("Pharmacy order {OrderId} remains in PendingPayment after payment expiry", orderId);
                    }
                    break;

                case "LabOrder":
                    var labOrder = await _unitOfWork.LabOrders.GetByIdAsync(orderId);
                    if (labOrder != null && labOrder.Status == LabOrderStatus.AwaitingPayment)
                    {
                        // LabOrder stays in AwaitingPayment — patient can retry
                        _logger.LogInformation("Lab order {OrderId} remains in AwaitingPayment after payment expiry", orderId);
                    }
                    break;
            }
        }
    }
}
