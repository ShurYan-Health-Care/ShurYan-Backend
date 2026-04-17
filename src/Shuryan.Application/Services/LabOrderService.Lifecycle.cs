using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{
    public partial class LabOrderService
    {
        #region Order Lifecycle

        public async Task<LabOrderResponse> ConfirmLabOrderAsync(Guid id)
        {
            try
            {
                var order = await _unitOfWork.LabOrders.GetByIdAsync(id);
                if (order == null)
                    throw new ArgumentException($"Lab order with ID {id} not found");

                // Can only confirm if in NewRequest status
                if (order.Status != LabOrderStatus.NewRequest)
                    throw new InvalidOperationException($"Cannot confirm order with status {order.Status}. Order must be in AwaitingLabReview status.");

                // Load the lab prescription with its items
                var labPrescription = await _unitOfWork.LabPrescriptions.GetByIdAsync(order.LabPrescriptionId);
                if (labPrescription == null)
                    throw new InvalidOperationException($"Lab prescription with ID {order.LabPrescriptionId} not found");

                // Calculate TestsTotalCost based on lab's pricing
                decimal testsTotalCost = 0;
                var prescriptionItems = await _unitOfWork.LabPrescriptionItems.GetAllAsync();
                var itemsForThisPrescription = prescriptionItems.Where(pi => pi.LabPrescriptionId == order.LabPrescriptionId).ToList();
                
                foreach (var prescItem in itemsForThisPrescription)
                {
                    // Get the lab's price for this test
                    var labServices = await _unitOfWork.LabServices.GetAllAsync();
                    var labService = labServices.FirstOrDefault(ls =>
                        ls.LaboratoryId == order.LaboratoryId &&
                        ls.LabTestId == prescItem.LabTestId);

                    if (labService != null)
                    {
                        testsTotalCost += labService.Price;
                    }
                }

                // Store the calculated cost
                order.TestsTotalCost = testsTotalCost;

                // Calculate and store sample collection delivery cost if applicable
                if (order.SampleCollectionType == SampleCollectionType.HomeSampleCollection)
                {
                    var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(order.LaboratoryId);
                    if (laboratory != null && laboratory.HomeSampleCollectionFee.HasValue)
                    {
                        order.SampleCollectionDeliveryCost = laboratory.HomeSampleCollectionFee.Value;
                    }
                }

                // Move to AwaitingPayment
                order.Status = LabOrderStatus.AwaitingPayment;
                order.ConfirmedByLabAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Confirmed lab order {OrderId} with TestsTotalCost={TestsTotalCost} and SampleCollectionDeliveryCost={SampleCollectionDeliveryCost}", 
                    id, order.TestsTotalCost, order.SampleCollectionDeliveryCost);

                return await GetLabOrderByIdAsync(id)
                    ?? throw new InvalidOperationException("Failed to retrieve confirmed lab order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming lab order {OrderId}", id);
                throw;
            }
        }

        public async Task<LabOrderResponse> MarkSampleCollectedAsync(Guid id)
        {
            try
            {
                var order = await _unitOfWork.LabOrders.GetByIdAsync(id);
                if (order == null)
                    throw new ArgumentException($"Lab order with ID {id} not found");

                // Can only collect samples if order is paid
                if (order.Status != LabOrderStatus.Paid
                )
                    throw new InvalidOperationException($"Cannot mark sample collected for order with status {order.Status}. Order must be Paid.");

                order.Status = LabOrderStatus.AwaitingSamples;
                order.SamplesCollectedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Marked sample collected for lab order {OrderId}", id);

                return await GetLabOrderByIdAsync(id)
                    ?? throw new InvalidOperationException("Failed to retrieve updated lab order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking sample collected for lab order {OrderId}", id);
                throw;
            }
        }

        public async Task<LabOrderResponse> MarkInProgressAsync(Guid id)
        {
            try
            {
                var order = await _unitOfWork.LabOrders.GetByIdAsync(id);
                if (order == null)
                    throw new ArgumentException($"Lab order with ID {id} not found");

                // Can only start processing if samples are collected
                if (order.Status != LabOrderStatus.AwaitingSamples && order.Status != LabOrderStatus.InProgressAtLab)
                    throw new InvalidOperationException($"Cannot mark in progress for order with status {order.Status}. Samples must be collected first.");

                order.Status = LabOrderStatus.InProgressAtLab;
                order.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Marked lab order {OrderId} as in progress", id);

                return await GetLabOrderByIdAsync(id)
                    ?? throw new InvalidOperationException("Failed to retrieve updated lab order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking lab order {OrderId} as in progress", id);
                throw;
            }
        }

        public async Task<LabOrderResponse> CompleteLabOrderAsync(Guid id)
        {
            try
            {
                var order = await _unitOfWork.LabOrders.GetByIdAsync(id);
                if (order == null)
                    throw new ArgumentException($"Lab order with ID {id} not found");

                if (order.Status != LabOrderStatus.InProgressAtLab && order.Status != LabOrderStatus.ResultsReady)
                    throw new InvalidOperationException($"Cannot complete order with status {order.Status}");

                order.Status = LabOrderStatus.Completed;
                order.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Completed lab order {OrderId}", id);

                return await GetLabOrderByIdAsync(id)
                    ?? throw new InvalidOperationException("Failed to retrieve completed lab order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing lab order {OrderId}", id);
                throw;
            }
        }

        /// <summary>
        /// [Laboratory] Start lab work - Move order from AwaitingSamples to InProgressAtLab
        /// Validates laboratory ownership and status transition
        /// </summary>
        public async Task<LabOrderResponse> StartLabWorkAsync(
            Guid orderId, 
            Guid laboratoryId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Get order
                var order = await _unitOfWork.LabOrders.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Lab order {OrderId} not found", orderId);
                    throw new ArgumentException($"طلب المعمل غير موجود");
                }

                // Verify laboratory ownership
                if (order.LaboratoryId != laboratoryId)
                {
                    _logger.LogWarning("Laboratory {LaboratoryId} attempted to access order {OrderId} belonging to laboratory {OrderLaboratoryId}",
                        laboratoryId, orderId, order.LaboratoryId);
                    throw new UnauthorizedAccessException("غير مصرح لك بالوصول لهذا الطلب");
                }

                // Verify current status
                if (order.Status != LabOrderStatus.AwaitingSamples)
                {
                    _logger.LogWarning("Cannot start lab work for order {OrderId} with status {Status}. Expected: AwaitingSamples",
                        orderId, order.Status);
                    throw new InvalidOperationException(
                        $"لا يمكن بدء العمل على الطلب. الحالة الحالية: {GetStatusDescription(order.Status)}. " +
                        $"يجب أن تكون الحالة: في انتظار العينات");
                }

                // Update status
                order.Status = LabOrderStatus.InProgressAtLab;
                order.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Laboratory {LaboratoryId} started work on order {OrderId} - Status changed from AwaitingSamples to InProgressAtLab",
                    laboratoryId, orderId);

                return await GetLabOrderByIdAsync(orderId)
                    ?? throw new InvalidOperationException("Failed to retrieve updated lab order");
            }
            catch (Exception ex) when (ex is ArgumentException or UnauthorizedAccessException or InvalidOperationException)
            {
                // Re-throw business logic exceptions
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting lab work for order {OrderId} by laboratory {LaboratoryId}",
                    orderId, laboratoryId);
                throw new InvalidOperationException("حدث خطأ أثناء بدء العمل على الطلب", ex);
            }
        }

        /// <summary>
        /// Get Arabic description for status
        /// </summary>
        private static string GetStatusDescription(LabOrderStatus status)
        {
            return status switch
            {
                LabOrderStatus.NewRequest => "طلب جديد",
                LabOrderStatus.AwaitingLabReview => "في انتظار مراجعة المعمل",
                LabOrderStatus.ConfirmedByLab => "تم التأكيد من المعمل",
                LabOrderStatus.AwaitingPayment => "في انتظار الدفع",
                LabOrderStatus.Paid => "تم الدفع",
                LabOrderStatus.AwaitingSamples => "في انتظار العينات",
                LabOrderStatus.InProgressAtLab => "قيد التنفيذ في المعمل",
                LabOrderStatus.ResultsReady => "النتائج جاهزة",
                LabOrderStatus.Completed => "تم الاستلام",
                LabOrderStatus.CancelledByPatient => "ملغي من المريض",
                LabOrderStatus.RejectedByLab => "مرفوض من المعمل",
                _ => status.ToString()
            };
        }

        #endregion

        #region Payment

        public async Task<LabOrderResponse> MarkLabOrderAsPaidAsync(Guid id, string paymentMethod, string? transactionId = null)
        {
            try
            {
                var order = await _unitOfWork.LabOrders.GetByIdAsync(id);
                if (order == null)
                    throw new ArgumentException($"Lab order with ID {id} not found");

                // Can only pay if waiting for payment
                if (order.Status != LabOrderStatus.AwaitingPayment)
                    throw new InvalidOperationException($"Cannot mark as paid for order with status {order.Status}. Order must be in AwaitingPayment status.");

                order.Status = LabOrderStatus.Paid;
                order.PaidAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Marked lab order {OrderId} as paid", id);

                return await GetLabOrderByIdAsync(id)
                    ?? throw new InvalidOperationException("Failed to retrieve paid lab order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking lab order {OrderId} as paid", id);
                throw;
            }
        }

        #endregion
    }
}
