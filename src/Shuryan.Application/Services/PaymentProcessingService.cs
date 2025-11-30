using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Paymob;
using Shuryan.Application.DTOs.Responses.Payment;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.External.Payments;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Enums.Laboratory;
using Shuryan.Core.Enums.Payment;
using Shuryan.Core.Enums.Pharmacy;
using Shuryan.Core.Interfaces.UnitOfWork;
using Shuryan.Core.Settings;

namespace Shuryan.Application.Services
{
    public class PaymentProcessingService : IPaymentProcessingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymobService _paymobService;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentProcessingService> _logger;
        private readonly PaymobSettings _paymobSettings;
        private readonly FrontendSettings _frontendSettings;
        private readonly IHostEnvironment _environment;

        public PaymentProcessingService(
            IUnitOfWork unitOfWork,
            IPaymobService paymobService,
            IMapper mapper,
            ILogger<PaymentProcessingService> logger,
            IOptions<PaymobSettings> paymobSettings,
            IOptions<FrontendSettings> frontendSettings,
            IHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _paymobService = paymobService;
            _mapper = mapper;
            _logger = logger;
            _paymobSettings = paymobSettings.Value;
            _frontendSettings = frontendSettings.Value;
            _environment = environment;
        }

        public async Task<ApiResponse<InitiatePaymentResponse>> InitiateAppointmentPaymentAsync(
            Guid userId,
            Guid appointmentId,
            PaymentMethod paymentMethod,
            PaymentType paymentType,
            string? ipAddress = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate appointment exists and belongs to user
                var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return ApiResponse<InitiatePaymentResponse>.Failure(
                        "الموعد غير موجود",
                        new[] { "Appointment not found" },
                        404);
                }

                if (appointment.PatientId != userId)
                {
                    return ApiResponse<InitiatePaymentResponse>.Failure(
                        "غير مصرح لك بالدفع لهذا الموعد",
                        new[] { "Unauthorized access to appointment" },
                        403);
                }

                // Check if already paid
                var existingPayment = await _unitOfWork.Payments
                    .GetPaymentsByOrderAsync("ConsultationBooking", appointmentId);
                
                if (existingPayment.Any(p => p.Status == PaymentStatus.Completed))
                {
                    return ApiResponse<InitiatePaymentResponse>.Failure(
                        "تم الدفع لهذا الموعد مسبقاً",
                        new[] { "Appointment already paid" },
                        400);
                }

                var amount = appointment.ConsultationFee;
                var itemName = $"حجز استشارة - موعد {appointment.Id}";
                var itemDescription = $"استشارة مع الدكتور - {appointment.SessionDurationMinutes} دقيقة";

                return await InitiatePaymentAsync(
                    userId,
                    "ConsultationBooking",
                    appointmentId,
                    amount,
                    paymentMethod,
                    paymentType,
                    itemName,
                    itemDescription,
                    ipAddress,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating appointment payment for user {UserId}, appointment {AppointmentId}",
                    userId, appointmentId);
                return ApiResponse<InitiatePaymentResponse>.Failure(
                    "حدث خطأ أثناء بدء عملية الدفع",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<InitiatePaymentResponse>> InitiatePharmacyOrderPaymentAsync(
            Guid userId,
            Guid pharmacyOrderId,
            PaymentMethod paymentMethod,
            PaymentType paymentType,
            string? ipAddress = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _unitOfWork.PharmacyOrders.GetByIdAsync(pharmacyOrderId);
                if (order == null)
                {
                    return ApiResponse<InitiatePaymentResponse>.Failure(
                        "الطلب غير موجود",
                        new[] { "Pharmacy order not found" },
                        404);
                }

                if (order.PatientId != userId)
                {
                    return ApiResponse<InitiatePaymentResponse>.Failure(
                        "غير مصرح لك بالدفع لهذا الطلب",
                        new[] { "Unauthorized access to order" },
                        403);
                }

                // Check if already paid
                var existingPayment = await _unitOfWork.Payments
                    .GetPaymentsByOrderAsync("PharmacyOrder", pharmacyOrderId);
                
                if (existingPayment.Any(p => p.Status == PaymentStatus.Completed))
                {
                    return ApiResponse<InitiatePaymentResponse>.Failure(
                        "تم الدفع لهذا الطلب مسبقاً",
                        new[] { "Order already paid" },
                        400);
                }

                var amount = order.TotalCost + order.DeliveryFee;
                var itemName = $"طلب صيدلية - {order.OrderNumber}";
                var itemDescription = $"طلب أدوية - إجمالي {amount} جنيه";

                return await InitiatePaymentAsync(
                    userId,
                    "PharmacyOrder",
                    pharmacyOrderId,
                    amount,
                    paymentMethod,
                    paymentType,
                    itemName,
                    itemDescription,
                    ipAddress,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating pharmacy order payment for user {UserId}, order {OrderId}",
                    userId, pharmacyOrderId);
                return ApiResponse<InitiatePaymentResponse>.Failure(
                    "حدث خطأ أثناء بدء عملية الدفع",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<InitiatePaymentResponse>> InitiateLabOrderPaymentAsync(
            Guid userId,
            Guid labOrderId,
            PaymentMethod paymentMethod,
            PaymentType paymentType,
            string? ipAddress = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _unitOfWork.LabOrders.GetByIdAsync(labOrderId);
                if (order == null)
                {
                    return ApiResponse<InitiatePaymentResponse>.Failure(
                        "الطلب غير موجود",
                        new[] { "Lab order not found" },
                        404);
                }

                if (order.PatientId != userId)
                {
                    return ApiResponse<InitiatePaymentResponse>.Failure(
                        "غير مصرح لك بالدفع لهذا الطلب",
                        new[] { "Unauthorized access to order" },
                        403);
                }

                // Check if already paid
                var existingPayment = await _unitOfWork.Payments
                    .GetPaymentsByOrderAsync("LabOrder", labOrderId);
                
                if (existingPayment.Any(p => p.Status == PaymentStatus.Completed))
                {
                    return ApiResponse<InitiatePaymentResponse>.Failure(
                        "تم الدفع لهذا الطلب مسبقاً",
                        new[] { "Order already paid" },
                        400);
                }

                var amount = order.TestsTotalCost + order.SampleCollectionDeliveryCost;
                var itemName = $"طلب معمل - {order.Id}";
                var itemDescription = $"تحاليل طبية - إجمالي {amount} جنيه";

                return await InitiatePaymentAsync(
                    userId,
                    "LabOrder",
                    labOrderId,
                    amount,
                    paymentMethod,
                    paymentType,
                    itemName,
                    itemDescription,
                    ipAddress,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating lab order payment for user {UserId}, order {OrderId}",
                    userId, labOrderId);
                return ApiResponse<InitiatePaymentResponse>.Failure(
                    "حدث خطأ أثناء بدء عملية الدفع",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<PaymentResponse>> HandlePaymobWebhookAsync(
            string hmac,
            string webhookJson,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Parse webhook data
                var webhookData = JsonSerializer.Deserialize<PaymobWebhookRequest>(webhookJson);
                if (webhookData?.Obj == null)
                {
                    return ApiResponse<PaymentResponse>.Failure(
                        "بيانات Webhook غير صحيحة",
                        new[] { "Invalid webhook data" },
                        400);
                }

                // Verify HMAC signature (skip in Development for testing)
                if (!_environment.IsDevelopment())
                {
                    if (!_paymobService.VerifyWebhookSignature(hmac, webhookData))
                    {
                        _logger.LogWarning("Invalid HMAC signature received from Paymob webhook");
                        return ApiResponse<PaymentResponse>.Failure(
                            "توقيع Webhook غير صحيح",
                            new[] { "Invalid HMAC signature" },
                            401);
                    }
                }
                else
                {
                    _logger.LogInformation("HMAC verification skipped in Development mode");
                }

                var transactionData = webhookData.Obj;
                var merchantOrderId = transactionData.Order?.MerchantOrderId;

                if (string.IsNullOrEmpty(merchantOrderId) || !Guid.TryParse(merchantOrderId, out var paymentId))
                {
                    _logger.LogWarning("Invalid merchant order ID in webhook: {MerchantOrderId}", merchantOrderId);
                    return ApiResponse<PaymentResponse>.Failure(
                        "معرف الطلب غير صحيح",
                        new[] { "Invalid merchant order ID" },
                        400);
                }

                // Get payment record
                var payment = await _unitOfWork.Payments.GetPaymentWithTransactionsAsync(paymentId);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for webhook: {PaymentId}", paymentId);
                    return ApiResponse<PaymentResponse>.Failure(
                        "عملية الدفع غير موجودة",
                        new[] { "Payment not found" },
                        404);
                }

                // Update payment based on transaction status
                if (transactionData.Success && !transactionData.Pending)
                {
                    await CompletePaymentAsync(payment, transactionData, cancellationToken);
                }
                else if (!transactionData.Success || transactionData.ErrorOccured)
                {
                    await FailPaymentAsync(payment, transactionData, cancellationToken);
                }
                else if (transactionData.Pending)
                {
                    await UpdatePaymentToProcessingAsync(payment, transactionData, cancellationToken);
                }

                var response = _mapper.Map<PaymentResponse>(payment);
                return ApiResponse<PaymentResponse>.Success(response, "تم معالجة Webhook بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Paymob webhook");
                return ApiResponse<PaymentResponse>.Failure(
                    "حدث خطأ أثناء معالجة Webhook",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<PaymentResponse>> GetPaymentByIdAsync(
            Guid paymentId,
            Guid userId,
            CancellationToken cancellationToken = default)
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

        public async Task<ApiResponse<PaymentResponse>> CancelPaymentAsync(
            Guid paymentId,
            Guid userId,
            CancellationToken cancellationToken = default)
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
                await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        #region Private Helper Methods

        private async Task<ApiResponse<InitiatePaymentResponse>> InitiatePaymentAsync(
            Guid userId,
            string orderType,
            Guid orderId,
            decimal amount,
            PaymentMethod paymentMethod,
            PaymentType paymentType,
            string itemName,
            string itemDescription,
            string? ipAddress,
            CancellationToken cancellationToken)
        {
            // Create payment record
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderType = orderType,
                OrderId = orderId,
                Amount = amount,
                PaymentMethod = paymentMethod,
                Provider = PaymentProvider.Paymob,
                Status = PaymentStatus.Pending,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create initial transaction
            var transaction = new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                TransactionType = "Initiation",
                Amount = amount,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<PaymentTransaction>().AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment initiated: {PaymentId} for user {UserId}, order type {OrderType}",
                payment.Id, userId, orderType);

            // Handle Cash on Delivery
            if (paymentMethod == PaymentMethod.CashOnDelivery)
            {
                var response = new InitiatePaymentResponse
                {
                    PaymentId = payment.Id,
                    RequiresRedirect = false,
                    Message = "سيتم الدفع عند الاستلام"
                };
                return ApiResponse<InitiatePaymentResponse>.Success(response, response.Message);
            }

            // Handle Paymob payment
            try
            {
                // Step 1: Authenticate
                var authToken = await _paymobService.AuthenticateAsync(cancellationToken);

                // Step 2: Create order
                var paymobOrder = await _paymobService.CreateOrderAsync(
                    authToken,
                    amount,
                    payment.Id.ToString(),
                    itemName,
                    itemDescription,
                    cancellationToken);

                // Step 3: Generate payment key
                var integrationId = paymentType == PaymentType.MobileWallet
                    ? _paymobSettings.MobileIntegrationId
                    : _paymobSettings.CardIntegrationId;

                // Get user details
                var user = await _unitOfWork.Repository<Core.Entities.Identity.User>().GetByIdAsync(userId);
                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }

                // Ensure all required fields have values (Paymob requirement)
                var userEmail = !string.IsNullOrWhiteSpace(user.Email) ? user.Email : "customer@shuryan.com";
                var userFirstName = !string.IsNullOrWhiteSpace(user.FirstName) ? user.FirstName : "Customer";
                var userLastName = !string.IsNullOrWhiteSpace(user.LastName) ? user.LastName : "User";
                var userPhone = !string.IsNullOrWhiteSpace(user.PhoneNumber) ? user.PhoneNumber : "01000000000";

                var paymentToken = await _paymobService.GeneratePaymentKeyAsync(
                    authToken,
                    paymobOrder.Id,
                    amount,
                    integrationId,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber,
                    cancellationToken);

                // Update payment with Paymob order ID
                payment.ProviderTransactionId = paymobOrder.Id.ToString();
                payment.Status = PaymentStatus.Processing;
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Generate iframe URL
                var iframeId = paymentType == PaymentType.MobileWallet 
                    ? int.Parse(_paymobSettings.MobileIFrameId) 
                    : int.Parse(_paymobSettings.CardIFrameId);
                var paymentUrl = _paymobService.GetIFrameUrl(paymentToken, iframeId);

                var successResponse = new InitiatePaymentResponse
                {
                    PaymentId = payment.Id,
                    PaymentUrl = paymentUrl,
                    RequiresRedirect = true,
                    ReferenceNumber = payment.Id.ToString(),
                    Message = "يرجى إكمال عملية الدفع من خلال الرابط المرفق"
                };

                return ApiResponse<InitiatePaymentResponse>.Success(successResponse, successResponse.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Paymob payment for {PaymentId}", payment.Id);
                
                // Mark payment as failed
                payment.Status = PaymentStatus.Failed;
                payment.FailedAt = DateTime.UtcNow;
                payment.FailureReason = ex.Message;
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<InitiatePaymentResponse>.Failure(
                    "حدث خطأ أثناء إنشاء عملية الدفع",
                    new[] { ex.Message },
                    500);
            }
        }

        private async Task CompletePaymentAsync(
            Payment payment,
            PaymobTransactionData transactionData,
            CancellationToken cancellationToken)
        {
            if (payment.Status == PaymentStatus.Completed)
            {
                _logger.LogInformation("Payment {PaymentId} already completed, skipping", payment.Id);
                return;
            }

            payment.Status = PaymentStatus.Completed;
            payment.CompletedAt = DateTime.UtcNow;
            payment.ProviderTransactionId = transactionData.Id.ToString();
            payment.ProviderResponse = JsonSerializer.Serialize(transactionData);

            // Create completion transaction
            var transaction = new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                TransactionType = "Completion",
                Amount = transactionData.AmountCents / 100m,
                Status = PaymentStatus.Completed,
                ProviderTransactionId = transactionData.Id.ToString(),
                ProviderResponse = JsonSerializer.Serialize(transactionData),
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<PaymentTransaction>().AddAsync(transaction);
            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update order status
            await UpdateOrderStatusAfterPaymentAsync(payment.OrderType, payment.OrderId, cancellationToken);

            _logger.LogInformation("Payment completed: {PaymentId}, Transaction: {TransactionId}",
                payment.Id, transactionData.Id);
        }

        private async Task FailPaymentAsync(
            Payment payment,
            PaymobTransactionData transactionData,
            CancellationToken cancellationToken)
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailedAt = DateTime.UtcNow;
            payment.FailureReason = transactionData.Data?.Message ?? "Payment failed";
            payment.ProviderResponse = JsonSerializer.Serialize(transactionData);

            // Create failure transaction
            var transaction = new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                TransactionType = "Failure",
                Amount = transactionData.AmountCents / 100m,
                Status = PaymentStatus.Failed,
                ProviderTransactionId = transactionData.Id.ToString(),
                ProviderResponse = JsonSerializer.Serialize(transactionData),
                ErrorMessage = transactionData.Data?.Message,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<PaymentTransaction>().AddAsync(transaction);
            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Payment failed: {PaymentId}, Reason: {Reason}",
                payment.Id, payment.FailureReason);
        }

        private async Task UpdatePaymentToProcessingAsync(
            Payment payment,
            PaymobTransactionData transactionData,
            CancellationToken cancellationToken)
        {
            if (payment.Status != PaymentStatus.Pending)
            {
                return;
            }

            payment.Status = PaymentStatus.Processing;
            payment.ProviderResponse = JsonSerializer.Serialize(transactionData);

            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment updated to processing: {PaymentId}", payment.Id);
        }

        private async Task UpdateOrderStatusAfterPaymentAsync(
            string orderType,
            Guid orderId,
            CancellationToken cancellationToken)
        {
            try
            {
                switch (orderType)
                {
                    case "ConsultationBooking":
                        var appointment = await _unitOfWork.Appointments.GetByIdAsync(orderId);
                        if (appointment != null)
                        {
                            // Update appointment status to Confirmed after successful payment
                            // Note: Appointment is already Confirmed by default, but we ensure it here
                            appointment.Status = AppointmentStatus.Confirmed;
                            appointment.UpdatedAt = DateTime.UtcNow;
                            _unitOfWork.Appointments.Update(appointment);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                            _logger.LogInformation("Appointment {AppointmentId} confirmed after payment", orderId);
                        }
                        break;

                    case "PharmacyOrder":
                        var pharmacyOrder = await _unitOfWork.PharmacyOrders.GetByIdAsync(orderId);
                        if (pharmacyOrder != null && pharmacyOrder.Status == PharmacyOrderStatus.PendingPayment)
                        {
                            pharmacyOrder.Status = PharmacyOrderStatus.Confirmed;
                            pharmacyOrder.UpdatedAt = DateTime.UtcNow;
                            _unitOfWork.PharmacyOrders.Update(pharmacyOrder);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                            _logger.LogInformation("Pharmacy order {OrderId} confirmed after payment", orderId);
                        }
                        break;

                    case "LabOrder":
                        var labOrder = await _unitOfWork.LabOrders.GetByIdAsync(orderId);
                        if (labOrder != null && labOrder.Status == LabOrderStatus.AwaitingPayment)
                        {
                            labOrder.Status = LabOrderStatus.AwaitingSamples;
                            labOrder.PaidAt = DateTime.UtcNow;
                            labOrder.UpdatedAt = DateTime.UtcNow;
                            _unitOfWork.LabOrders.Update(labOrder);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                            _logger.LogInformation("Lab order {OrderId} marked as AwaitingSamples after successful payment", orderId);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status after payment for {OrderType} {OrderId}",
                    orderType, orderId);
            }
        }

        /// <summary>
        /// [TEST ONLY] Simulate successful payment - Updates order status directly without real payment
        /// </summary>
        public async Task<ApiResponse<string>> SimulatePaymentSuccessAsync(
            Guid userId,
            string orderType,
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogWarning("[TEST MODE] Simulating payment success for {OrderType} {OrderId} by user {UserId}",
                    orderType, orderId, userId);

                // Verify order exists and belongs to user
                switch (orderType)
                {
                    case "LabOrder":
                        var labOrder = await _unitOfWork.LabOrders.GetByIdAsync(orderId);
                        if (labOrder == null)
                        {
                            return ApiResponse<string>.Failure(
                                "طلب المعمل غير موجود",
                                new[] { "Lab order not found" },
                                404);
                        }

                        if (labOrder.PatientId != userId)
                        {
                            return ApiResponse<string>.Failure(
                                "غير مصرح لك بالوصول لهذا الطلب",
                                new[] { "Unauthorized access" },
                                403);
                        }

                        if (labOrder.Status != LabOrderStatus.AwaitingPayment)
                        {
                            return ApiResponse<string>.Failure(
                                $"حالة الطلب الحالية: {labOrder.Status}. يجب أن تكون 'في انتظار الدفع'",
                                new[] { $"Current status: {labOrder.Status}" },
                                400);
                        }

                        // Update to AwaitingSamples
                        labOrder.Status = LabOrderStatus.AwaitingSamples;
                        labOrder.PaidAt = DateTime.UtcNow;
                        labOrder.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.LabOrders.Update(labOrder);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("[TEST MODE] Lab order {OrderId} updated to AwaitingSamples", orderId);
                        return ApiResponse<string>.Success(
                            "تم تحديث حالة الطلب بنجاح - في انتظار العينات",
                            "تم محاكاة الدفع بنجاح (TEST MODE)");

                    case "PharmacyOrder":
                        var pharmacyOrder = await _unitOfWork.PharmacyOrders.GetByIdAsync(orderId);
                        if (pharmacyOrder == null)
                        {
                            return ApiResponse<string>.Failure(
                                "طلب الصيدلية غير موجود",
                                new[] { "Pharmacy order not found" },
                                404);
                        }

                        if (pharmacyOrder.PatientId != userId)
                        {
                            return ApiResponse<string>.Failure(
                                "غير مصرح لك بالوصول لهذا الطلب",
                                new[] { "Unauthorized access" },
                                403);
                        }

                        pharmacyOrder.Status = PharmacyOrderStatus.Confirmed;
                        pharmacyOrder.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.PharmacyOrders.Update(pharmacyOrder);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("[TEST MODE] Pharmacy order {OrderId} confirmed", orderId);
                        return ApiResponse<string>.Success(
                            "تم تأكيد الطلب بنجاح",
                            "تم محاكاة الدفع بنجاح (TEST MODE)");

                    case "ConsultationBooking":
                        var appointment = await _unitOfWork.Appointments.GetByIdAsync(orderId);
                        if (appointment == null)
                        {
                            return ApiResponse<string>.Failure(
                                "الموعد غير موجود",
                                new[] { "Appointment not found" },
                                404);
                        }

                        if (appointment.PatientId != userId)
                        {
                            return ApiResponse<string>.Failure(
                                "غير مصرح لك بالوصول لهذا الموعد",
                                new[] { "Unauthorized access" },
                                403);
                        }

                        appointment.Status = AppointmentStatus.Confirmed;
                        appointment.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.Appointments.Update(appointment);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("[TEST MODE] Appointment {OrderId} confirmed", orderId);
                        return ApiResponse<string>.Success(
                            "تم تأكيد الموعد بنجاح",
                            "تم محاكاة الدفع بنجاح (TEST MODE)");

                    default:
                        return ApiResponse<string>.Failure(
                            "نوع الطلب غير مدعوم",
                            new[] { "Unsupported order type" },
                            400);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TEST MODE] Error simulating payment success for {OrderType} {OrderId}",
                    orderType, orderId);
                return ApiResponse<string>.Failure(
                    "حدث خطأ أثناء محاكاة الدفع",
                    new[] { ex.Message },
                    500);
            }
        }

        #endregion
    }
}
