using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    public interface ILaboratoryService
    {
        #region CRUD Operations

        /// <summary>
        /// Get all laboratories with optional filters
        /// </summary>
        Task<IEnumerable<LaboratoryResponse>> GetAllLaboratoriesAsync(
            string? searchTerm = null,
            bool? offersHomeSampleCollection = null,
            bool includeInactive = false);

        /// <summary>
        /// Get laboratory by ID with full details
        /// </summary>
        Task<LaboratoryResponse?> GetLaboratoryByIdAsync(Guid id);

        /// <summary>
        /// Get laboratory basic info
        /// </summary>
        Task<LaboratoryBasicResponse?> GetLaboratoryBasicInfoAsync(Guid id);

        /// <summary>
        /// Create a new laboratory
        /// </summary>
        Task<LaboratoryResponse> CreateLaboratoryAsync(CreateLaboratoryRequest request);

        /// <summary>
        /// Update laboratory information
        /// </summary>
        Task<LaboratoryResponse> UpdateLaboratoryAsync(Guid id, UpdateLaboratoryRequest request);

        /// <summary>
        /// Delete laboratory (soft delete)
        /// </summary>
        Task<bool> DeleteLaboratoryAsync(Guid id);

        #endregion

        #region Lab Services Management

        /// <summary>
        /// Get all services offered by a laboratory
        /// </summary>
        Task<IEnumerable<LabServiceResponse>> GetLaboratoryServicesAsync(Guid laboratoryId);

        /// <summary>
        /// Add a new service to laboratory
        /// </summary>
        Task<LabServiceResponse> AddLaboratoryServiceAsync(Guid laboratoryId, CreateLabServiceRequest request);

        /// <summary>
        /// Update laboratory service
        /// </summary>
        Task<LabServiceResponse> UpdateLaboratoryServiceAsync(Guid serviceId, CreateLabServiceRequest request);

        /// <summary>
        /// Remove service from laboratory
        /// </summary>
        Task<bool> RemoveLaboratoryServiceAsync(Guid serviceId);

        #endregion

        #region Working Hours

        /// <summary>
        /// Get laboratory working hours
        /// </summary>
        Task<IEnumerable<LabWorkingHoursResponse>> GetLaboratoryWorkingHoursAsync(Guid laboratoryId);

        /// <summary>
        /// Set laboratory working hours
        /// </summary>
        Task SetLaboratoryWorkingHoursAsync(Guid laboratoryId, IEnumerable<CreateLabWorkingHoursRequest> request);

        #endregion

        #region Search & Filter

        /// <summary>
        /// Search laboratories by location
        /// </summary>
        Task<IEnumerable<LaboratoryResponse>> SearchLaboratoriesByLocationAsync(
            double latitude,
            double longitude,
            double radiusKm);

        /// <summary>
        /// Get laboratories offering specific test
        /// </summary>
        Task<IEnumerable<LaboratoryResponse>> GetLaboratoriesOfferingTestAsync(Guid labTestId);

        #endregion

        #region Statistics

        /// <summary>
        /// Get laboratory statistics
        /// </summary>
        Task<LaboratoryStatistics> GetLaboratoryStatisticsAsync(Guid laboratoryId);

        #endregion
    }

    public class LaboratoryStatistics
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalServices { get; set; }
    }
}
