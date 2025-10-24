using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    public interface ILabPrescriptionService
    {
        #region CRUD Operations

        /// <summary>
        /// Get all lab prescriptions with optional filters
        /// </summary>
        Task<IEnumerable<LabPrescriptionResponse>> GetAllLabPrescriptionsAsync(
            Guid? doctorId = null,
            Guid? patientId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// Get lab prescription by ID with full details
        /// </summary>
        Task<LabPrescriptionResponse?> GetLabPrescriptionByIdAsync(Guid id);

        /// <summary>
        /// Get lab prescription by appointment ID
        /// </summary>
        Task<LabPrescriptionResponse?> GetLabPrescriptionByAppointmentIdAsync(Guid appointmentId);

        /// <summary>
        /// Get patient's lab prescriptions
        /// </summary>
        Task<IEnumerable<LabPrescriptionResponse>> GetPatientLabPrescriptionsAsync(Guid patientId);

        /// <summary>
        /// Get doctor's lab prescriptions
        /// </summary>
        Task<IEnumerable<LabPrescriptionResponse>> GetDoctorLabPrescriptionsAsync(Guid doctorId);

        /// <summary>
        /// Create a new lab prescription
        /// </summary>
        Task<LabPrescriptionResponse> CreateLabPrescriptionAsync(CreateLabPrescriptionRequest request);

        /// <summary>
        /// Update lab prescription
        /// </summary>
        Task<LabPrescriptionResponse> UpdateLabPrescriptionAsync(Guid id, CreateLabPrescriptionRequest request);

        /// <summary>
        /// Delete lab prescription
        /// </summary>
        Task<bool> DeleteLabPrescriptionAsync(Guid id);

        #endregion

        #region Prescription Items Management

        /// <summary>
        /// Get prescription items
        /// </summary>
        Task<IEnumerable<LabPrescriptionItemResponse>> GetPrescriptionItemsAsync(Guid prescriptionId);

        /// <summary>
        /// Add item to prescription
        /// </summary>
        Task<LabPrescriptionItemResponse> AddPrescriptionItemAsync(Guid prescriptionId, CreateLabPrescriptionItemRequest request);

        /// <summary>
        /// Remove item from prescription
        /// </summary>
        Task<bool> RemovePrescriptionItemAsync(Guid itemId);

        #endregion
    }
}
