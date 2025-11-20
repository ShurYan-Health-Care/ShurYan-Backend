using Microsoft.AspNetCore.Http;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Pharmacy;
using Shuryan.Application.DTOs.Responses.Pharmacy;

namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// واجهة خدمة إدارة بروفايل الصيدلية
    /// </summary>
    public interface IPharmacyProfileService
    {
        #region Basic Info Operations
        
        /// <summary>
        /// جلب المعلومات الأساسية للصيدلية
        /// </summary>
        Task<PharmacyBasicInfoResponse?> GetBasicInfoAsync(Guid pharmacyId);

        /// <summary>
        /// تحديث المعلومات الأساسية للصيدلية
        /// </summary>
        Task<PharmacyBasicInfoResponse> UpdateBasicInfoAsync(Guid pharmacyId, UpdatePharmacyBasicInfoRequest request);

        /// <summary>
        /// رفع صورة البروفايل للصيدلية
        /// </summary>
        Task<string> UpdateProfileImageAsync(Guid pharmacyId, IFormFile image);

        #endregion

        #region Address Operations

        /// <summary>
        /// جلب عنوان الصيدلية
        /// </summary>
        Task<PharmacyAddressResponse?> GetAddressAsync(Guid pharmacyId);

        /// <summary>
        /// تحديث عنوان الصيدلية
        /// </summary>
        Task<PharmacyAddressResponse> UpdateAddressAsync(Guid pharmacyId, UpdatePharmacyAddressRequest request);

        #endregion

        #region Working Hours Operations

        /// <summary>
        /// جلب ساعات وأيام عمل الصيدلية
        /// </summary>
        Task<PharmacyWorkingHoursResponse> GetWorkingHoursAsync(Guid pharmacyId);

        /// <summary>
        /// تحديث ساعات وأيام عمل الصيدلية
        /// </summary>
        Task<PharmacyWorkingHoursResponse> UpdateWorkingHoursAsync(Guid pharmacyId, UpdatePharmacyWorkingHoursRequest request);

        #endregion

        #region Delivery Operations

        /// <summary>
        /// جلب إعدادات التوصيل للصيدلية
        /// </summary>
        Task<PharmacyDeliveryResponse> GetDeliverySettingsAsync(Guid pharmacyId);

        /// <summary>
        /// تحديث إعدادات التوصيل للصيدلية
        /// </summary>
        Task<PharmacyDeliveryResponse> UpdateDeliverySettingsAsync(Guid pharmacyId, UpdatePharmacyDeliveryRequest request);

        #endregion

        #region Prescription Operations

        /// <summary>
        /// جلب الروشتات المعلقة (Pending) للصيدلية
        /// </summary>
        Task<PendingPrescriptionsListResponse> GetPendingPrescriptionsAsync(Guid pharmacyId);

        /// <summary>
        /// جلب تفاصيل روشتة معينة
        /// </summary>
        Task<PrescriptionDetailsResponse> GetPrescriptionDetailsAsync(Guid pharmacyId, Guid orderId);

        #endregion

        #region Orders Operations

        /// <summary>
        /// جلب جميع الطلبات الخاصة بالصيدلية مع pagination
        /// </summary>
        Task<PharmacyOrdersListResponse> GetPharmacyOrdersAsync(Guid pharmacyId, PaginationParams paginationParams);

        /// <summary>
        /// رد الصيدلية على طلب الروشتة بتوفر الأدوية والأسعار
        /// </summary>
        Task<PharmacyOrderResponseResponse> RespondToOrderAsync(Guid pharmacyId, Guid orderId, PharmacyOrderResponseRequest request);

        /// <summary>
        /// جلب إحصائيات الصيدلية
        /// </summary>
        Task<PharmacyStatisticsResponse> GetPharmacyStatisticsAsync(Guid pharmacyId);

        /// <summary>
        /// جلب قائمة الطلبات المحسّنة للصيدلية
        /// </summary>
        Task<PharmacyOrdersListOptimizedResponse> GetOptimizedOrdersAsync(Guid pharmacyId, int pageNumber, int pageSize);

        /// <summary>
        /// جلب تفاصيل طلب معين
        /// </summary>
        Task<PharmacyOrderDetailResponse> GetOrderDetailAsync(Guid pharmacyId, Guid orderId);

        /// <summary>
        /// تحديث حالة طلب صيدلية إلى واحدة من الحالات التالية فقط:
        /// PreparationInProgress, OutForDelivery, ReadyForPickup, Delivered
        /// </summary>
        Task<PharmacyOrderStatusUpdateResponse> UpdateOrderStatusAsync(Guid pharmacyId, Guid orderId, UpdatePharmacyOrderStatusRequest request);

        #endregion

    }
}
