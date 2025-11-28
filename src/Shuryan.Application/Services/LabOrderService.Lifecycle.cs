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

                // Move to ConfirmedByLab first, then to AwaitingPayment
                order.Status = LabOrderStatus.AwaitingPayment;
                order.ConfirmedByLabAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Confirmed lab order {OrderId} and moved to AwaitingPayment", id);

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
