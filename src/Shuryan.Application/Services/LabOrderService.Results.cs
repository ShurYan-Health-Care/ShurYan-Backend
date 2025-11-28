using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{
    public partial class LabOrderService
    {
        #region Results Management

        public async Task<IEnumerable<LabResultResponse>> GetLabOrderResultsAsync(Guid labOrderId)
        {
            try
            {
                var results = await _unitOfWork.LabResults.GetAllAsync();
                var orderResults = results.Where(r => r.LabOrderId == labOrderId).ToList();

                var responses = _mapper.Map<IEnumerable<LabResultResponse>>(orderResults);
                _logger.LogInformation("Retrieved {Count} results for lab order {OrderId}", responses.Count(), labOrderId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting results for lab order {OrderId}", labOrderId);
                throw;
            }
        }

        public async Task<LabResultResponse> AddLabOrderResultAsync(Guid labOrderId, CreateLabResultRequest request)
        {
            try
            {
                var order = await _unitOfWork.LabOrders.GetByIdAsync(labOrderId);
                if (order == null)
                    throw new ArgumentException($"Lab order with ID {labOrderId} not found");

                var labTest = await _unitOfWork.LabTests.GetByIdAsync(request.LabTestId);
                if (labTest == null)
                    throw new ArgumentException($"Lab test with ID {request.LabTestId} not found");

                var labResult = _mapper.Map<LabResult>(request);
                labResult.Id = Guid.NewGuid();
                labResult.LabOrderId = labOrderId;
                labResult.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.LabResults.AddAsync(labResult);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Added result to lab order {OrderId}", labOrderId);

                var response = _mapper.Map<LabResultResponse>(labResult);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding result to lab order {OrderId}", labOrderId);
                throw;
            }
        }

        public async Task<LabResultResponse> UpdateLabResultAsync(Guid resultId, UpdateLabResultRequest request)
        {
            try
            {
                var result = await _unitOfWork.LabResults.GetByIdAsync(resultId);
                if (result == null)
                    throw new ArgumentException($"Lab result with ID {resultId} not found");

                // Update properties
                if (!string.IsNullOrEmpty(request.ResultValue))
                    result.ResultValue = request.ResultValue;

                if (request.ReferenceRange != null)
                    result.ReferenceRange = request.ReferenceRange;

                if (request.Unit != null)
                    result.Unit = request.Unit;

                if (request.Notes != null)
                    result.Notes = request.Notes;

                result.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Updated lab result {ResultId}", resultId);

                var response = _mapper.Map<LabResultResponse>(result);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lab result {ResultId}", resultId);
                throw;
            }
        }

        #endregion

        #region Statistics

        public async Task<LabOrderStatistics> GetLabOrderStatisticsAsync(
            Guid? laboratoryId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var orders = (await _unitOfWork.LabOrders.GetAllAsync()).ToList();

                // Apply filters
                if (laboratoryId.HasValue)
                    orders = orders.Where(o => o.LaboratoryId == laboratoryId.Value).ToList();

                if (startDate.HasValue)
                    orders = orders.Where(o => o.CreatedAt >= startDate.Value).ToList();

                if (endDate.HasValue)
                    orders = orders.Where(o => o.CreatedAt <= endDate.Value).ToList();

                var statistics = new LabOrderStatistics
                {
                    TotalOrders = orders.Count,
                    PendingPaymentOrders = orders.Count(o => o.Status == LabOrderStatus.NewRequest || 
                                                           o.Status == LabOrderStatus.AwaitingLabReview || 
                                                           o.Status == LabOrderStatus.AwaitingPayment),
                    ConfirmedOrders = orders.Count(o => o.Status == LabOrderStatus.ConfirmedByLab || 
                                                   o.Status == LabOrderStatus.Paid),
                    SampleCollectedOrders = orders.Count(o => o.Status == LabOrderStatus.AwaitingSamples),
                    InProgressOrders = orders.Count(o => o.Status == LabOrderStatus.InProgressAtLab || 
                                                    o.Status == LabOrderStatus.ResultsReady),
                    CompletedOrders = orders.Count(o => o.Status == LabOrderStatus.Completed),
                    CancelledOrders = orders.Count(o => o.Status == LabOrderStatus.CancelledByPatient || 
                                                   o.Status == LabOrderStatus.RejectedByLab),
                    TotalRevenue = orders.Where(o => o.Status == LabOrderStatus.Completed)
                                         .Sum(o => o.TestsTotalCost + o.SampleCollectionDeliveryCost),
                    AverageOrderValue = orders.Any()
                        ? orders.Average(o => o.TestsTotalCost + o.SampleCollectionDeliveryCost)
                        : 0
                };

                _logger.LogInformation("Retrieved lab order statistics");
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab order statistics");
                throw;
            }
        }

        #endregion
    }
}
