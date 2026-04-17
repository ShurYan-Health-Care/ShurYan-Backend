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

        /// <summary>
        /// [Laboratory] Submit lab results for an order and mark as completed
        /// </summary>
        public async Task<LabOrderResponse> SubmitLabResultsAsync(
            Guid orderId, 
            Guid laboratoryId, 
            SubmitLabResultsRequest request, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Get the order with details
                var order = await _unitOfWork.LabOrders.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Lab order not found: {OrderId}", orderId);
                    throw new ArgumentException($"Lab order with ID {orderId} not found");
                }

                // 2. Verify the order belongs to the authenticated laboratory
                if (order.LaboratoryId != laboratoryId)
                {
                    _logger.LogWarning("Laboratory {LaboratoryId} attempted to submit results for order {OrderId} belonging to laboratory {OwnerLaboratoryId}", 
                        laboratoryId, orderId, order.LaboratoryId);
                    throw new UnauthorizedAccessException("This order does not belong to your laboratory");
                }

                // 3. Verify order status is InProgressAtLab
                if (order.Status != LabOrderStatus.InProgressAtLab)
                {
                    _logger.LogWarning("Cannot submit results for order {OrderId} with status {Status}", orderId, order.Status);
                    throw new InvalidOperationException($"Cannot submit results for order with status {order.Status}. Order must be in InProgressAtLab status");
                }

                // 4. Get the lab prescription to verify test IDs
                var labPrescription = await _unitOfWork.LabPrescriptions.GetAsync(
                    p => p.Id == order.LabPrescriptionId, 
                    "Items");
                
                if (labPrescription == null)
                {
                    _logger.LogWarning("Lab prescription not found for order {OrderId}", orderId);
                    throw new InvalidOperationException("Lab prescription not found for this order");
                }

                // Get all test IDs from the prescription
                var prescriptionTestIds = labPrescription.Items.Select(i => i.LabTestId).ToHashSet();

                // 5. Verify all submitted labTestIds exist in the order's prescription
                var invalidTestIds = request.Results
                    .Where(r => !prescriptionTestIds.Contains(r.LabTestId))
                    .Select(r => r.LabTestId)
                    .ToList();

                if (invalidTestIds.Any())
                {
                    _logger.LogWarning("Invalid lab test IDs submitted for order {OrderId}: {InvalidTestIds}", 
                        orderId, string.Join(", ", invalidTestIds));
                    throw new ArgumentException($"The following lab test IDs are not part of this order: {string.Join(", ", invalidTestIds)}");
                }

                // 6. Create LabResult entities for each result
                var labResults = new List<LabResult>();
                foreach (var resultDto in request.Results)
                {
                    var labResult = new LabResult
                    {
                        Id = Guid.NewGuid(),
                        LabOrderId = orderId,
                        LabTestId = resultDto.LabTestId,
                        ResultValue = resultDto.ResultValue,
                        ReferenceRange = resultDto.ReferenceRange,
                        Unit = resultDto.Unit,
                        Notes = resultDto.Notes,
                        AttachmentUrl = resultDto.AttachmentUrl,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.LabResults.AddAsync(labResult);
                    labResults.Add(labResult);
                }

                // 7. Update order status to Completed
                order.Status = LabOrderStatus.Completed;
                order.UpdatedAt = DateTime.UtcNow;

                // 8. Save all changes
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully submitted {Count} results for order {OrderId} and marked as completed", 
                    labResults.Count, orderId);

                // 9. Return updated order response
                var response = await GetLabOrderByIdAsync(orderId);
                if (response == null)
                {
                    throw new InvalidOperationException("Failed to retrieve updated lab order");
                }

                return response;
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not InvalidOperationException && ex is not UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Error submitting results for lab order {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// [Patient/Doctor] Get lab order results with authorization check
        /// </summary>
        public async Task<LabOrderResultsResponse> GetLabOrderResultsWithAuthAsync(
            Guid orderId, 
            Guid userId, 
            string userRole, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Get the order
                var order = await _unitOfWork.LabOrders.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Lab order not found: {OrderId}", orderId);
                    throw new ArgumentException($"Lab order with ID {orderId} not found");
                }

                // 2. Authorization check based on role
                bool isAuthorized = false;

                if (userRole == "Patient")
                {
                    // Patient must own the order
                    isAuthorized = order.PatientId == userId;
                    if (!isAuthorized)
                    {
                        _logger.LogWarning("Patient {PatientId} attempted to access order {OrderId} belonging to patient {OwnerPatientId}",
                            userId, orderId, order.PatientId);
                    }
                }
                else if (userRole == "Doctor")
                {
                    // Doctor must have prescribed it - check via prescription
                    var prescription = await _unitOfWork.LabPrescriptions.GetByIdAsync(order.LabPrescriptionId);
                    if (prescription != null)
                    {
                        isAuthorized = prescription.DoctorId == userId;
                        if (!isAuthorized)
                        {
                            _logger.LogWarning("Doctor {DoctorId} attempted to access order {OrderId} prescribed by doctor {PrescribingDoctorId}",
                                userId, orderId, prescription.DoctorId);
                        }
                    }
                }

                if (!isAuthorized)
                {
                    throw new UnauthorizedAccessException("You are not authorized to view this order's results");
                }

                // 3. Get patient info
                var patient = await _unitOfWork.Patients.GetByIdAsync(order.PatientId);
                string patientName = patient != null ? $"{patient.FirstName} {patient.LastName}" : "Unknown";

                // 4. Get laboratory info
                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(order.LaboratoryId);
                string laboratoryName = laboratory?.Name ?? "Unknown";

                // 5. Get lab results
                var labResults = await _unitOfWork.LabResults.GetResultsByLabOrderAsync(orderId);
                var resultDetails = new List<LabResultDetailDto>();

                foreach (var result in labResults)
                {
                    var labTest = await _unitOfWork.LabTests.GetByIdAsync(result.LabTestId);
                    if (labTest != null)
                    {
                        // Simple abnormal detection: check if result value is numeric and outside reference range
                        bool isAbnormal = DetectAbnormalResult(result.ResultValue, result.ReferenceRange);

                        resultDetails.Add(new LabResultDetailDto
                        {
                            LabResultId = result.Id,
                            TestId = result.LabTestId,
                            TestName = labTest.Name,
                            TestCode = labTest.Code,
                            Category = labTest.Category.ToString(),
                            ResultValue = result.ResultValue,
                            ReferenceRange = result.ReferenceRange,
                            Unit = result.Unit,
                            Notes = result.Notes,
                            AttachmentUrl = result.AttachmentUrl,
                            IsAbnormal = isAbnormal
                        });
                    }
                }

                // 6. Build response
                var response = new LabOrderResultsResponse
                {
                    OrderId = order.Id,
                    PatientName = patientName,
                    LaboratoryName = laboratoryName,
                    OrderDate = order.CreatedAt,
                    CompletedDate = order.Status == LabOrderStatus.Completed ? order.UpdatedAt : null,
                    Status = (int)order.Status,
                    StatusName = order.Status.ToString(),
                    Results = resultDetails
                };

                _logger.LogInformation("Retrieved {Count} results for order {OrderId} by {Role} {UserId}",
                    resultDetails.Count, orderId, userRole, userId);

                return response;
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Error retrieving results for lab order {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Simple abnormal detection based on numeric range
        /// </summary>
        private bool DetectAbnormalResult(string resultValue, string? referenceRange)
        {
            if (string.IsNullOrEmpty(referenceRange))
                return false;

            // Try to parse result as decimal
            if (!decimal.TryParse(resultValue, out decimal numericResult))
                return false;

            // Try to parse reference range (format: "min-max")
            var rangeParts = referenceRange.Split('-');
            if (rangeParts.Length != 2)
                return false;

            if (decimal.TryParse(rangeParts[0].Trim(), out decimal min) &&
                decimal.TryParse(rangeParts[1].Trim(), out decimal max))
            {
                return numericResult < min || numericResult > max;
            }

            return false;
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
