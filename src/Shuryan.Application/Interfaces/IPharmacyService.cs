using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Application.DTOs.Requests.Pharmacy;
using Shuryan.Application.DTOs.Responses.Pharmacy;
using Shuryan.Application.DTOs.Responses.Review;
using Shuryan.Core.Enums;

namespace Shuryan.Application.Interfaces
{
    public interface IPharmacyService
    {
        #region Pharmacy Profile Management
        Task<PharmacyResponse?> GetPharmacyByIdAsync(Guid id);
        Task<PharmacyResponse?> GetPharmacyByEmailAsync(string email);
        Task<bool> DeletePharmacyAsync(Guid id);
        Task<PharmacyResponse?> GetCurrentPharmacyAsync(Guid pharmacyId);
        Task<PharmacyResponse> UpdatePharmacyAsync(Guid id, UpdatePharmacyRequest request);
        #endregion

        #region Working Hours Management
        Task<IEnumerable<PharmacyWorkingHoursResponse>> GetWorkingHoursAsync(Guid pharmacyId);
        Task<PharmacyWorkingHoursResponse> AddWorkingHoursAsync(Guid pharmacyId, CreatePharmacyWorkingHoursRequest request);
        Task<PharmacyWorkingHoursResponse> UpdateWorkingHoursAsync(Guid pharmacyId, Guid workingHoursId, UpdatePharmacyWorkingHoursRequest request);
        Task<bool> DeleteWorkingHoursAsync(Guid pharmacyId, Guid workingHoursId);
        Task<IsOpenResponse> IsPharmacyOpenAsync(Guid pharmacyId);
        Task<PharmacyOrderResponse> AcceptOrderAsync(Guid pharmacyId, Guid orderId);
        Task<PharmacyOrderResponse> RejectOrderAsync(Guid pharmacyId, Guid orderId, RejectOrderRequest request);
        Task<PharmacyOrderResponse> MarkOrderAsPreparingAsync(Guid pharmacyId, Guid orderId);
        Task<PharmacyOrderResponse> MarkOrderAsReadyAsync(Guid pharmacyId, Guid orderId);
        Task<PharmacyOrderResponse> DispatchOrderAsync(Guid pharmacyId, Guid orderId);
        Task<PharmacyOrderResponse> MarkOrderAsDeliveredAsync(Guid pharmacyId, Guid orderId);
        #endregion

        #region Orders Management
        Task<IEnumerable<PharmacyOrderResponse>> GetPharmacyOrdersAsync(Guid pharmacyId, int pageNumber = 1, int pageSize = 20);
        Task<PharmacyOrderResponse?> GetOrderByIdAsync(Guid pharmacyId, Guid orderId);
        Task<PharmacyOrderResponse> UpdateOrderStatusAsync(Guid pharmacyId, Guid orderId, UpdateOrderStatusRequest request);
        Task<IEnumerable<PharmacyOrderResponse>> GetPendingOrdersAsync(Guid pharmacyId);
        Task<IEnumerable<PharmacyOrderResponse>> GetInProgressOrdersAsync(Guid pharmacyId);
        Task<IEnumerable<PharmacyOrderResponse>> GetCompletedOrdersAsync(Guid pharmacyId);
        Task<IEnumerable<PharmacyResponse>> SearchPharmaciesAsync(SearchPharmaciesRequest request);
        Task<IEnumerable<PharmacyResponse>> GetPharmaciesByGovernorateAsync(Governorate governorate);
        Task<IEnumerable<PharmacyResponse>> GetNearbyPharmaciesAsync(NearbyPharmaciesRequest request);
        #endregion

        #region Document Management
        Task<IEnumerable<PharmacyDocumentResponse>> GetPharmacyDocumentsAsync(Guid pharmacyId);
        Task<PharmacyDocumentResponse> UploadDocumentAsync(Guid pharmacyId, CreatePharmacyDocumentRequest request);
        #endregion

        #region Reviews Management
        Task<IEnumerable<PharmacyReviewResponse>> GetPharmacyReviewsAsync(Guid pharmacyId);
        Task<PharmacyReviewStatisticsResponse> GetReviewStatisticsAsync(Guid pharmacyId);
        #endregion

        #region Address Management
        Task<AddressResponse?> GetPharmacyAddressAsync(Guid pharmacyId);
        #endregion

        #region Prescription Handling
        Task<PrescriptionDetailsResponse?> GetPrescriptionDetailsAsync(Guid pharmacyId, Guid prescriptionId);
        #endregion

        #region Verification Management
        Task<IEnumerable<PharmacyResponse>> GetPendingVerificationPharmaciesAsync();
        Task<PharmacyResponse> VerifyPharmacyAsync(Guid pharmacyId, VerifyPharmacyRequest request);
        Task<PharmacyResponse> RejectVerificationAsync(Guid pharmacyId, RejectVerificationRequest request);
        Task<PharmacyDocumentResponse> ApproveDocumentAsync(Guid pharmacyId, Guid documentId, ApproveDocumentRequest request);
        Task<PharmacyDocumentResponse> RejectDocumentAsync(Guid pharmacyId, Guid documentId, RejectDocumentRequest request);
        #endregion
    }
}
