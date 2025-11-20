using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Application.DTOs.Responses.Pharmacy;
using Shuryan.Application.DTOs.Responses.Prescription;

namespace Shuryan.Application.Interfaces
{
    public interface IPatientService
    {
        #region Basic CRUD Operations
        Task<PatientResponse?> GetPatientByIdAsync(Guid id);
        Task<PatientResponse?> GetPatientByEmailAsync(string Email);
        Task<bool> DeletePatientAsync(Guid id);
        Task<bool> RestorePatientAsync(Guid id);
        Task<PatientResponse?> GetCurrentPatientAsync(Guid userId);
        Task<PatientResponse> UpdatePatientAsync(Guid id, UpdatePatientRequest request);
        Task<PatientResponse> CreatePatientAsync(CreatePatientRequest request);
        #endregion

        #region Query Operations
        Task<IEnumerable<PatientResponse>> GetAllPatientsAsync(bool includeDeleted = false);
        Task<PaginatedResponse<PatientResponse>> GetPaginatedPatientsAsync(PaginationParams request);
        Task<PaginatedResponse<PatientResponse>> SearchPatientsAsync(SearchTermPatientsRequest request);
        Task<IEnumerable<PatientResponse>> GetPatientsWithMedicalHistoryAsync();
        Task<bool> IsEmailUniqueAsync(string email);
        Task<int> GetTotalPatientsCountAsync(bool includeDeleted = false);
        #endregion

        #region Medical History Operations
        Task<IEnumerable<MedicalHistoryItemResponse>> GetPatientMedicalHistoryAsync(Guid patientId);
        Task<MedicalHistoryItemResponse> AddMedicalHistoryItemAsync(Guid patientId, CreateMedicalHistoryItemRequest request);
        Task<MedicalHistoryItemResponse> UpdateMedicalHistoryItemAsync(Guid patientId, Guid itemId, UpdateMedicalHistoryItemRequest request);
        Task<bool> DeleteMedicalHistoryItemAsync(Guid patientId, Guid itemId);
        #endregion

        #region Medical Record Operations (Profile)
        Task<MedicalRecordResponse?> GetPatientMedicalRecordAsync(Guid patientId);
        Task<MedicalRecordResponse> UpdatePatientMedicalRecordAsync(Guid patientId, UpdateMedicalRecordRequest request);
        #endregion

        #region Appointments Operations
        Task<IEnumerable<AppointmentResponse>> GetPatientAppointmentsAsync(Guid patientId);
        Task<IEnumerable<AppointmentResponse>> GetUpcomingAppointmentsAsync(Guid patientId);
        Task<IEnumerable<AppointmentResponse>> GetPastAppointmentsAsync(Guid patientId);
        Task<AppointmentResponse?> GetNextAppointmentAsync(Guid patientId);
        Task<int> GetAppointmentsCountAsync(Guid patientId);
        #endregion

        #region Prescriptions Operations
        Task<IEnumerable<PrescriptionResponse>> GetPatientPrescriptionsAsync(Guid patientId);
        Task<IEnumerable<PrescriptionResponse>> GetActivePrescriptionsAsync(Guid patientId);
        Task<PrescriptionResponse?> GetPrescriptionByIdAsync(Guid patientId, Guid prescriptionId);
        #endregion

        #region Lab Orders Operations
        Task<IEnumerable<LabOrderResponse>> GetPatientLabOrdersAsync(Guid patientId);
        Task<IEnumerable<LabOrderResponse>> GetPendingLabOrdersAsync(Guid patientId);
        Task<LabOrderResponse?> GetLabOrderByIdAsync(Guid patientId, Guid orderId);
        #endregion

        #region Address Operations
        Task<AddressResponse?> GetPatientAddressAsync(Guid patientId);
        Task<AddressResponse> UpdatePatientAddressAsync(Guid patientId, UpdateAddressRequest request);
        Task<AddressResponse> CreatePatientAddressAsync(CreateAddressRequest request); // Keep original signature
        #endregion

        #region Profile Operations
        Task<bool> UpdateProfileImageAsync(Guid patientId, string imageUrl);
        Task<bool> RemoveProfileImageAsync(Guid patientId);
        #endregion

        #region Pharmacy Operations
        Task<FindNearbyPharmaciesResponse> FindNearbyPharmaciesAsync(FindNearbyPharmaciesRequest request);
        Task<FindNearbyPharmaciesResponse> FindNearbyPharmaciesForPatientAsync(Guid patientId);
        Task<SendPrescriptionResponse> SendPrescriptionToPharmacyAsync(Guid patientId, Guid prescriptionId, SendPrescriptionToPharmacyRequest request);
        Task<PatientPharmacyResponseView> GetPharmacyResponseAsync(Guid patientId, Guid orderId);
        Task<PrescriptionPharmacyResponsesView> GetPrescriptionPharmacyResponsesAsync(Guid patientId, Guid prescriptionId);
        Task<ConfirmOrderResponse> ConfirmPharmacyOrderAsync(Guid patientId, Guid orderId);
        #endregion
    }
}
