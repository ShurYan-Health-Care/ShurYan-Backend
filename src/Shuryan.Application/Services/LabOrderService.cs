using AutoMapper;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Enums.Laboratory;
using Shuryan.Core.Interfaces.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{
    public partial class LabOrderService : ILabOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LabOrderService> _logger;

        public LabOrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<LabOrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        #region Get All Lab Orders
        public async Task<IEnumerable<LabOrderResponse>> GetAllLabOrdersAsync(
            Guid? patientId = null,
            Guid? laboratoryId = null,
            LabOrderStatus? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var orders = (await _unitOfWork.LabOrders.GetAllAsync()).ToList();

                // Apply filters
                if (patientId.HasValue)
                    orders = orders.Where(o => o.PatientId == patientId.Value).ToList();

                if (laboratoryId.HasValue)
                    orders = orders.Where(o => o.LaboratoryId == laboratoryId.Value).ToList();

                if (status.HasValue)
                    orders = orders.Where(o => o.Status == status.Value).ToList();

                if (startDate.HasValue)
                    orders = orders.Where(o => o.CreatedAt >= startDate.Value).ToList();

                if (endDate.HasValue)
                    orders = orders.Where(o => o.CreatedAt <= endDate.Value).ToList();

                var responses = new List<LabOrderResponse>();
                foreach (var order in orders)
                {
                    // Use GetLabOrderByIdAsync to get full details
                    var response = await GetLabOrderByIdAsync(order.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved {Count} lab orders", responses.Count);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all lab orders");
                throw;
            }
        }
        #endregion

        #region Get Lab Order By Id
        public async Task<LabOrderResponse?> GetLabOrderByIdAsync(Guid id)
        {
            try
            {
                var order = await _unitOfWork.LabOrders.GetOrderWithDetailsAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("Lab order with ID {OrderId} not found", id);
                    return null;
                }

                var response = _mapper.Map<LabOrderResponse>(order);

                // Load Patient Name
                var patient = await _unitOfWork.Patients.GetByIdAsync(order.PatientId);
                if (patient != null)
                {
                    response.PatientName = $"{patient.FirstName} {patient.LastName}";
                }

                // Load Laboratory Name
                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(order.LaboratoryId);
                if (laboratory != null)
                {
                    response.LaboratoryName = laboratory.Name;
                }

                // Load Lab Prescription Tests
                var labPrescription = await _unitOfWork.LabPrescriptions.GetByIdAsync(order.LabPrescriptionId);
                if (labPrescription != null)
                {
                    var tests = new List<LabOrderTestResponse>();
                    decimal totalCost = 0;

                    foreach (var prescItem in labPrescription.Items)
                    {
                        var labTest = await _unitOfWork.LabTests.GetByIdAsync(prescItem.LabTestId);
                        if (labTest != null)
                        {
                            // Get price from laboratory's services
                            var labServices = await _unitOfWork.LabServices.GetAllAsync();
                            var labService = labServices.FirstOrDefault(ls =>
                                ls.LaboratoryId == order.LaboratoryId &&
                                ls.LabTestId == labTest.Id);

                            decimal price = labService?.Price ?? 0;
                            totalCost += price;

                            tests.Add(new LabOrderTestResponse
                            {
                                LabTestId = labTest.Id,
                                LabTestName = labTest.Name,
                                LabTestCategory = labTest.Category.ToString(),
                                Price = price
                            });
                        }
                    }

                    response.Tests = tests;
                    response.TestsTotalCost = totalCost;

                    // Add home collection fee if applicable
                    if (order.SampleCollectionType == SampleCollectionType.HomeSampleCollection && laboratory != null)
                    {
                        response.SampleCollectionDeliveryCost = laboratory.HomeSampleCollectionFee ?? 0;
                    }
                }

                _logger.LogInformation("Retrieved lab order {OrderId}", id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab order {OrderId}", id);
                throw;
            }
        }
        #endregion

        #region Get Patient & Laboratory Orders
        public async Task<IEnumerable<LabOrderResponse>> GetPatientLabOrdersAsync(Guid patientId)
        {
            try
            {
                var orders = await _unitOfWork.LabOrders.GetPagedOrdersForPatientAsync(patientId, 1, 100);

                var responses = new List<LabOrderResponse>();
                foreach (var order in orders)
                {
                    var response = await GetLabOrderByIdAsync(order.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved lab orders for patient {PatientId}", patientId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab orders for patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<LabOrderResponse>> GetLaboratoryLabOrdersAsync(Guid laboratoryId)
        {
            try
            {
                var orders = await _unitOfWork.LabOrders.GetPagedOrdersForLaboratoryAsync(laboratoryId, null, 1, 100);

                var responses = new List<LabOrderResponse>();
                foreach (var order in orders)
                {
                    var response = await GetLabOrderByIdAsync(order.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved lab orders for laboratory {LaboratoryId}", laboratoryId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab orders for laboratory {LaboratoryId}", laboratoryId);
                throw;
            }
        }

        public async Task<IEnumerable<LabOrderResponse>> GetPatientActiveLabOrdersAsync(Guid patientId)
        {
            try
            {
                var orders = await _unitOfWork.LabOrders.GetPagedOrdersForPatientAsync(patientId, 1, 100);
                
                // Filter for active statuses: NewRequest, AwaitingLabReview, ConfirmedByLab, AwaitingPayment, 
                // Paid, AwaitingSamples, InProgressAtLab, CancelledByPatient, RejectedByLab
                var activeStatuses = new[]
                {
                    LabOrderStatus.NewRequest,
                    LabOrderStatus.AwaitingLabReview,
                    LabOrderStatus.ConfirmedByLab,
                    LabOrderStatus.AwaitingPayment,
                    LabOrderStatus.Paid,
                    LabOrderStatus.AwaitingSamples,
                    LabOrderStatus.InProgressAtLab,
                    LabOrderStatus.CancelledByPatient,
                    LabOrderStatus.RejectedByLab
                };

                var filteredOrders = orders.Where(o => activeStatuses.Contains(o.Status));

                var responses = new List<LabOrderResponse>();
                foreach (var order in filteredOrders)
                {
                    var response = await GetLabOrderByIdAsync(order.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved {Count} active lab orders for patient {PatientId}", responses.Count, patientId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active lab orders for patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<LabOrderResponse>> GetPatientCompletedLabOrdersAsync(Guid patientId)
        {
            try
            {
                var orders = await _unitOfWork.LabOrders.GetPagedOrdersForPatientAsync(patientId, 1, 100);
                
                // Filter for completed statuses: ResultsReady, Completed
                var completedStatuses = new[]
                {
                    LabOrderStatus.ResultsReady,
                    LabOrderStatus.Completed
                };

                var filteredOrders = orders.Where(o => completedStatuses.Contains(o.Status));

                var responses = new List<LabOrderResponse>();
                foreach (var order in filteredOrders)
                {
                    var response = await GetLabOrderByIdAsync(order.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved {Count} completed lab orders for patient {PatientId}", responses.Count, patientId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting completed lab orders for patient {PatientId}", patientId);
                throw;
            }
        }
        #endregion

        #region Create Order
        public async Task<LabOrderResponse> CreateLabOrderAsync(CreateLabOrderRequest request)
        {
            try
            {
                // Validate lab prescription exists
                var labPrescription = await _unitOfWork.LabPrescriptions.GetByIdAsync(request.LabPrescriptionId);
                if (labPrescription == null)
                    throw new ArgumentException($"Lab prescription with ID {request.LabPrescriptionId} not found");

                // Check if this prescription already has an order
                var existingOrders = await _unitOfWork.LabOrders.GetAllAsync();
                var existingOrder = existingOrders.FirstOrDefault(o => o.LabPrescriptionId == request.LabPrescriptionId);
                if (existingOrder != null)
                    throw new InvalidOperationException($"هذه الروشتة تم إنشاء طلب لها بالفعل (Order ID: {existingOrder.Id})");

                // Validate laboratory exists
                var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(request.LaboratoryId);
                if (laboratory == null)
                    throw new ArgumentException($"Laboratory with ID {request.LaboratoryId} not found");

                var labOrder = _mapper.Map<LabOrder>(request);
                labOrder.Id = Guid.NewGuid();
                labOrder.Status = LabOrderStatus.NewRequest;
                labOrder.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.LabOrders.AddAsync(labOrder);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created lab order {OrderId}", labOrder.Id);

                return await GetLabOrderByIdAsync(labOrder.Id)
                    ?? throw new InvalidOperationException("Failed to retrieve created lab order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lab order");
                throw;
            }
        }
        #endregion

        #region Update Order
        public async Task<LabOrderResponse> UpdateLabOrderStatusAsync(Guid id, LabOrderStatus newStatus, string? notes = null)
        {
            try
            {
                var order = await _unitOfWork.LabOrders.GetByIdAsync(id);
                if (order == null)
                    throw new ArgumentException($"Lab order with ID {id} not found");

                order.Status = newStatus;
                order.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated lab order {OrderId} status to {Status}", id, newStatus);

                return await GetLabOrderByIdAsync(id)
                    ?? throw new InvalidOperationException("Failed to retrieve updated lab order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lab order status {OrderId}", id);
                throw;
            }
        }
        #endregion

        #region Cancel & Reject
        public async Task<LabOrderResponse> CancelLabOrderAsync(Guid id, string cancellationReason)
        {
            try
            {
                var order = await _unitOfWork.LabOrders.GetByIdAsync(id);
                if (order == null)
                    throw new ArgumentException($"Lab order with ID {id} not found");

                if (order.Status == LabOrderStatus.Completed)
                    throw new InvalidOperationException("Cannot cancel a completed lab order");

                if (order.Status == LabOrderStatus.CancelledByPatient || order.Status == LabOrderStatus.RejectedByLab)
                    throw new InvalidOperationException("Lab order is already cancelled or rejected");

                order.Status = LabOrderStatus.CancelledByPatient;
                order.CancellationReason = cancellationReason;
                order.CancelledAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Cancelled lab order {OrderId} by patient", id);

                return await GetLabOrderByIdAsync(id)
                    ?? throw new InvalidOperationException("Failed to retrieve cancelled lab order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling lab order {OrderId}", id);
                throw;
            }
        }

        public async Task<LabOrderResponse> RejectLabOrderAsync(Guid id, string rejectionReason)
        {
            try
            {
                var order = await _unitOfWork.LabOrders.GetByIdAsync(id);
                if (order == null)
                    throw new ArgumentException($"Lab order with ID {id} not found");

                if (order.Status == LabOrderStatus.Completed)
                    throw new InvalidOperationException("Cannot reject a completed lab order");

                if (order.Status == LabOrderStatus.CancelledByPatient || order.Status == LabOrderStatus.RejectedByLab)
                    throw new InvalidOperationException("Lab order is already cancelled or rejected");

                order.Status = LabOrderStatus.RejectedByLab;
                order.RejectionReason = rejectionReason;
                order.RejectedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Rejected lab order {OrderId} by laboratory", id);

                return await GetLabOrderByIdAsync(id)
                    ?? throw new InvalidOperationException("Failed to retrieve rejected lab order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting lab order {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteLabOrderAsync(Guid id)
        {
            try
            {
                var order = await _unitOfWork.LabOrders.GetByIdAsync(id);
                if (order == null)
                    return false;

                // Mark as rejected instead of soft delete
                order.Status = LabOrderStatus.RejectedByLab;
                order.RejectionReason = "Deleted by administrator";
                order.RejectedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted lab order {OrderId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lab order {OrderId}", id);
                throw;
            }
        }
        #endregion

        #region Get Orders By Prescription
        public async Task<IEnumerable<LabOrderResponse>> GetLabOrdersByPrescriptionAsync(Guid prescriptionId)
        {
            try
            {
                var orders = await _unitOfWork.LabOrders.GetAllAsync();
                var filteredOrders = orders.Where(o => o.LabPrescriptionId == prescriptionId);

                var responses = new List<LabOrderResponse>();
                foreach (var order in filteredOrders)
                {
                    var response = await GetLabOrderByIdAsync(order.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved {Count} lab orders for prescription {PrescriptionId}", responses.Count, prescriptionId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab orders for prescription {PrescriptionId}", prescriptionId);
                throw;
            }
        }
        #endregion

        #region Get Lab Results
        public async Task<IEnumerable<LabResultResponse>> GetLabResultsAsync(Guid labOrderId)
        {
            try
            {
                var results = await _unitOfWork.LabResults.GetResultsByLabOrderAsync(labOrderId);
                var responses = new List<LabResultResponse>();

                foreach (var result in results)
                {
                    var labTest = await _unitOfWork.LabTests.GetByIdAsync(result.LabTestId);
                    responses.Add(new LabResultResponse
                    {
                        Id = result.Id,
                        LabOrderId = result.LabOrderId,
                        LabTestId = result.LabTestId,
                        TestName = labTest?.Name ?? string.Empty,
                        TestCode = labTest?.Code ?? string.Empty,
                        ResultValue = result.ResultValue,
                        ReferenceRange = result.ReferenceRange,
                        Unit = result.Unit,
                        Notes = result.Notes,
                        AttachmentUrl = result.AttachmentUrl,
                        CreatedAt = result.CreatedAt
                    });
                }

                _logger.LogInformation("Retrieved {Count} results for lab order {OrderId}", responses.Count, labOrderId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting results for lab order {OrderId}", labOrderId);
                throw;
            }
        }
        #endregion
    }
}
