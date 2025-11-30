using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    public interface ILabOrderService
    {
        #region CRUD Operations

        /// <summary>
        /// Get all lab orders with optional filters
        /// </summary>
        Task<IEnumerable<LabOrderResponse>> GetAllLabOrdersAsync(
            Guid? patientId = null,
            Guid? laboratoryId = null,
            LabOrderStatus? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// Get lab order by ID with full details
        /// </summary>
        Task<LabOrderResponse?> GetLabOrderByIdAsync(Guid id);

        /// <summary>
        /// Get patient's lab orders
        /// </summary>
        Task<IEnumerable<LabOrderResponse>> GetPatientLabOrdersAsync(Guid patientId);

        /// <summary>
        /// Get patient's active lab orders (NewRequest, AwaitingLabReview, ConfirmedByLab, AwaitingPayment, Paid, AwaitingSamples, InProgressAtLab, CancelledByPatient, RejectedByLab)
        /// </summary>
        Task<IEnumerable<LabOrderResponse>> GetPatientActiveLabOrdersAsync(Guid patientId);

        /// <summary>
        /// Get patient's completed lab orders (ResultsReady, Completed)
        /// </summary>
        Task<IEnumerable<LabOrderResponse>> GetPatientCompletedLabOrdersAsync(Guid patientId);

        /// <summary>
        /// Get laboratory's lab orders
        /// </summary>
        Task<IEnumerable<LabOrderResponse>> GetLaboratoryLabOrdersAsync(Guid laboratoryId);

        /// <summary>
        /// Get lab orders by prescription ID
        /// </summary>
        Task<IEnumerable<LabOrderResponse>> GetLabOrdersByPrescriptionAsync(Guid prescriptionId);

        /// <summary>
        /// Get lab results for an order
        /// </summary>
        Task<IEnumerable<LabResultResponse>> GetLabResultsAsync(Guid labOrderId);

        /// <summary>
        /// Create a new lab order
        /// </summary>
        Task<LabOrderResponse> CreateLabOrderAsync(CreateLabOrderRequest request);

        /// <summary>
        /// Update lab order status
        /// </summary>
        Task<LabOrderResponse> UpdateLabOrderStatusAsync(Guid id, LabOrderStatus newStatus, string? notes = null);

        /// <summary>
        /// Cancel lab order
        /// </summary>
        Task<LabOrderResponse> CancelLabOrderAsync(Guid id, string cancellationReason);

        /// <summary>
        /// Reject lab order by laboratory
        /// </summary>
        Task<LabOrderResponse> RejectLabOrderAsync(Guid id, string rejectionReason);

        /// <summary>
        /// Delete lab order (soft delete)
        /// </summary>
        Task<bool> DeleteLabOrderAsync(Guid id);

        #endregion

        #region Order Lifecycle

        /// <summary>
        /// Confirm lab order by laboratory
        /// </summary>
        Task<LabOrderResponse> ConfirmLabOrderAsync(Guid id);

        /// <summary>
        /// Mark order as sample collected
        /// </summary>
        Task<LabOrderResponse> MarkSampleCollectedAsync(Guid id);

        /// <summary>
        /// Mark order as in progress (tests being performed)
        /// </summary>
        Task<LabOrderResponse> MarkInProgressAsync(Guid id);

        /// <summary>
        /// [Laboratory] Start lab work - Transition from AwaitingSamples to InProgressAtLab
        /// </summary>
        Task<LabOrderResponse> StartLabWorkAsync(Guid orderId, Guid laboratoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Complete lab order (all results ready)
        /// </summary>
        Task<LabOrderResponse> CompleteLabOrderAsync(Guid id);

        #endregion

        #region Results Management

        /// <summary>
        /// Get lab order results
        /// </summary>
        Task<IEnumerable<LabResultResponse>> GetLabOrderResultsAsync(Guid labOrderId);

        /// <summary>
        /// Add result to lab order
        /// </summary>
        Task<LabResultResponse> AddLabOrderResultAsync(Guid labOrderId, CreateLabResultRequest request);

        /// <summary>
        /// Update lab result
        /// </summary>
        Task<LabResultResponse> UpdateLabResultAsync(Guid resultId, UpdateLabResultRequest request);

        #endregion

        #region Payment

        /// <summary>
        /// Mark lab order as paid
        /// </summary>
        Task<LabOrderResponse> MarkLabOrderAsPaidAsync(Guid id, string paymentMethod, string? transactionId = null);

        #endregion

        #region Statistics

        /// <summary>
        /// Get lab order statistics
        /// </summary>
        Task<LabOrderStatistics> GetLabOrderStatisticsAsync(
            Guid? laboratoryId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        #endregion
    }

    public class LabOrderStatistics
    {
        public int TotalOrders { get; set; }
        public int PendingPaymentOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int SampleCollectedOrders { get; set; }
        public int InProgressOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
