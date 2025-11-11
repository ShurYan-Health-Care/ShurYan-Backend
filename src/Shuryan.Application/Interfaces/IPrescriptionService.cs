using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Prescription;
using Shuryan.Application.DTOs.Responses.Prescription;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    public interface IPrescriptionService
    {
        #region Basic CRUD Operations

        /// <summary>
        /// Get prescription by ID with full details
        /// </summary>
        Task<PrescriptionResponse?> GetPrescriptionByIdAsync(Guid id);

        /// <summary>
        /// Create a new prescription
        /// </summary>
        Task<PrescriptionResponse> CreatePrescriptionAsync(CreatePrescriptionRequest request);

        /// <summary>
        /// Update prescription instructions
        /// </summary>
        Task<PrescriptionResponse> UpdatePrescriptionAsync(Guid id, UpdatePrescriptionRequest request);

        /// <summary>
        /// Delete prescription (hard delete)
        /// </summary>
        Task<bool> DeletePrescriptionAsync(Guid id);

        #endregion

        #region Prescription Lifecycle Operations

        /// <summary>
        /// Cancel a prescription with reason
        /// </summary>
        Task<PrescriptionResponse> CancelPrescriptionAsync(Guid id, CancelPrescriptionRequest request);

        /// <summary>
        /// Renew an existing prescription
        /// </summary>
        Task<PrescriptionResponse> RenewPrescriptionAsync(Guid id, RenewPrescriptionRequest request);

        #endregion

        #region Query Operations

        /// <summary>
        /// Advanced query with multiple filters
        /// </summary>
        Task<PrescriptionQueryResponse> GetPrescriptionsAsync(PrescriptionQueryParams queryParams);

        /// <summary>
        /// Get all prescriptions with pagination
        /// </summary>
        Task<PaginatedResponse<PrescriptionResponse>> GetPaginatedPrescriptionsAsync(PaginationParams request);

        /// <summary>
        /// Get prescription by prescription number
        /// </summary>
        Task<PrescriptionResponse?> GetPrescriptionByNumberAsync(string prescriptionNumber);

        /// <summary>
        /// Get total prescriptions count
        /// </summary>
        Task<int> GetTotalPrescriptionsCountAsync();

        #endregion

        #region Patient Related Operations

        /// <summary>
        /// Get all prescriptions for a specific patient
        /// </summary>
        Task<IEnumerable<PrescriptionResponse>> GetPatientPrescriptionsAsync(Guid patientId);

        /// <summary>
        /// Get paginated prescriptions for a specific patient
        /// </summary>
        Task<PaginatedResponse<PrescriptionResponse>> GetPaginatedPatientPrescriptionsAsync(
            Guid patientId,
            PaginationParams request);

        /// <summary>
        /// Get active prescriptions for a patient (not ordered yet)
        /// </summary>
        Task<IEnumerable<PrescriptionResponse>> GetActivePatientPrescriptionsAsync(Guid patientId);

        /// <summary>
        /// Get prescriptions count for a patient
        /// </summary>
        Task<int> GetPatientPrescriptionsCountAsync(Guid patientId);

        /// <summary>
        /// Get patient's current active medications
        /// </summary>
        Task<IEnumerable<CurrentMedicationResponse>> GetCurrentMedicationsAsync(Guid patientId);

        #endregion

        #region Doctor Related Operations

        /// <summary>
        /// Get all prescriptions created by a specific doctor
        /// </summary>
        Task<IEnumerable<PrescriptionResponse>> GetDoctorPrescriptionsAsync(Guid doctorId);

        /// <summary>
        /// Get paginated prescriptions for a specific doctor
        /// </summary>
        Task<PaginatedResponse<PrescriptionResponse>> GetPaginatedDoctorPrescriptionsAsync(
            Guid doctorId,
            PaginationParams request);

        /// <summary>
        /// Get prescriptions count for a doctor
        /// </summary>
        Task<int> GetDoctorPrescriptionsCountAsync(Guid doctorId);

        #endregion

        #region Pharmacy Related Operations

        /// <summary>
        /// Get prescriptions containing a specific medication
        /// </summary>
        Task<IEnumerable<PrescriptionResponse>> GetPrescriptionsContainingMedicationAsync(Guid medicationId);

        /// <summary>
        /// Mark prescription as dispensed
        /// </summary>
        Task<bool> MarkPrescriptionAsDispensedAsync(Guid prescriptionId);

        /// <summary>
        /// Verify prescription authenticity
        /// </summary>
        //Task<PrescriptionVerificationResponse> VerifyPrescriptionAsync(Guid id, string? verificationCode);

        /// <summary>
        /// Dispense prescription at pharmacy
        /// </summary>
        //Task<DispenseResult> DispensePrescriptionAsync(Guid id, DispensePrescriptionRequest request);

        /// <summary>
        /// Share prescription with pharmacy
        /// </summary>
        //Task<SharePrescriptionResult> SharePrescriptionAsync(Guid id, SharePrescriptionRequest request);

        /// <summary>
        /// Get dispensing history for a prescription
        /// </summary>
        //Task<IEnumerable<DispensingRecord>> GetDispensingHistoryAsync(Guid id);

        /// <summary>
        /// Accept prescription for delivery
        /// </summary>
        //Task AcceptPrescriptionDeliveryAsync(Guid id, AcceptDeliveryRequest request);

        #endregion

        #region Analytics & Statistics Operations

        /// <summary>
        /// Get prescription status history (audit trail)
        /// </summary>
        //Task<IEnumerable<PrescriptionStatusHistory>> GetStatusHistoryAsync(Guid id);

        #endregion

        #region Date Range Operations

        /// <summary>
        /// Get prescriptions created within a date range
        /// </summary>
        Task<IEnumerable<PrescriptionResponse>> GetPrescriptionsByDateRangeAsync(
            DateTime startDate,
            DateTime endDate);

        /// <summary>
        /// Get prescriptions created within a date range for a specific doctor
        /// </summary>
        Task<IEnumerable<PrescriptionResponse>> GetDoctorPrescriptionsByDateRangeAsync(
            Guid doctorId,
            DateTime startDate,
            DateTime endDate);

        /// <summary>
        /// Get prescriptions created within a date range for a specific patient
        /// </summary>
        Task<IEnumerable<PrescriptionResponse>> GetPatientPrescriptionsByDateRangeAsync(
            Guid patientId,
            DateTime startDate,
            DateTime endDate);

        #endregion

        #region Appointment Related Operations

        /// <summary>
        /// Get prescription for a specific appointment
        /// </summary>
        Task<PrescriptionResponse?> GetPrescriptionByAppointmentIdAsync(Guid appointmentId);

        /// <summary>
        /// Check if appointment already has a prescription
        /// </summary>
        Task<bool> AppointmentHasPrescriptionAsync(Guid appointmentId);

        #endregion

        #region Patient-Doctor Prescription Operations

        /// <summary>
        /// Get a specific prescription between patient and doctor with full medication details
        /// </summary>
        Task<PrescriptionDetailedResponse?> GetPrescriptionBetweenPatientAndDoctorAsync(
            Guid prescriptionId,
            Guid patientId,
            Guid doctorId);

        /// <summary>
        /// Get all prescriptions between patient and doctor (summary list)
        /// </summary>
        Task<IEnumerable<PrescriptionListItemResponse>> GetPrescriptionListBetweenPatientAndDoctorAsync(
            Guid patientId,
            Guid doctorId);

        #endregion

        #region Medication Operations

        /// <summary>
        /// Get all medication names available in the system with optional search
        /// </summary>
        /// <param name="searchTerm">Optional search term to filter medications by name</param>
        Task<IEnumerable<MedicationNameResponse>> GetAllMedicationNamesAsync(string? searchTerm = null);

        #endregion

        #region Patient Prescriptions

        /// <summary>
        /// Get all prescriptions for a patient from all doctors (for profile page)
        /// </summary>
        Task<IEnumerable<PatientPrescriptionListResponse>> GetPatientPrescriptionsListAsync(Guid patientId);

        #endregion
    }
}
