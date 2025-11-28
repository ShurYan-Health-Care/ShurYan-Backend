using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Core.Enums.Laboratory;

namespace Shuryan.Application.Interfaces
{
        /// <summary>
        /// خدمات المعامل للمريض
        /// </summary>
        public interface IPatientLabService
        {
                #region Laboratory Search

                /// <summary>
                /// البحث عن أقرب 3 معامل للمريض بناءً على الإحداثيات (Simple - زي الصيدليات)
                /// </summary>
                Task<FindNearbyLaboratoriesResponse> FindNearbyLaboratoriesAsync(FindNearbyLaboratoriesRequest request);

                /// <summary>
                /// البحث عن أقرب 3 معامل للمريض بناءً على عنوانه المسجل
                /// </summary>
                Task<FindNearbyLaboratoriesResponse> FindNearbyLaboratoriesForPatientAsync(Guid patientId);

                /// <summary>
                /// البحث عن معامل قريبة (Old - مع فلاتر كثيرة)
                /// </summary>
                Task<IEnumerable<NearbyLaboratoryResponse>> GetNearbyLaboratoriesAsync(
                    Guid patientId,
                    double latitude,
                    double longitude,
                    double radiusInKm = 10,
                    bool? offersHomeSampleCollection = null,
                    string? searchQuery = null,
                    int pageNumber = 1,
                    int pageSize = 20);

                /// <summary>
                /// جلب تفاصيل معمل
                /// </summary>
                Task<LaboratoryDetailResponse?> GetLaboratoryDetailsAsync(Guid patientId, Guid laboratoryId, double? latitude = null, double? longitude = null);

                /// <summary>
                /// جلب خدمات معمل
                /// </summary>
                Task<IEnumerable<LaboratoryServiceResponse>> GetLaboratoryServicesAsync(Guid laboratoryId, string? category = null);

                /// <summary>
                /// جلب تقييمات معمل
                /// </summary>
                Task<IEnumerable<LaboratoryReviewResponse>> GetLaboratoryReviewsAsync(Guid laboratoryId, int pageNumber = 1, int pageSize = 20);

                #endregion

                #region Lab Prescriptions

                /// <summary>
                /// جلب روشتات التحاليل للمريض
                /// </summary>
                Task<IEnumerable<PatientLabPrescriptionResponse>> GetPatientLabPrescriptionsAsync(Guid patientId);

                /// <summary>
                /// جلب تفاصيل روشتة تحاليل
                /// </summary>
                Task<PatientLabPrescriptionResponse?> GetLabPrescriptionDetailsAsync(Guid patientId, Guid prescriptionId);

                #endregion

                #region Lab Orders

                /// <summary>
                /// إنشاء طلب تحاليل
                /// </summary>
                Task<PatientLabOrderResponse> CreateLabOrderAsync(Guid patientId, CreatePatientLabOrderRequest request);

                /// <summary>
                /// جلب طلبات التحاليل للمريض
                /// </summary>
                Task<IEnumerable<PatientLabOrderResponse>> GetPatientLabOrdersAsync(Guid patientId, LabOrderStatus? status = null);

                /// <summary>
                /// جلب تفاصيل طلب تحاليل
                /// </summary>
                Task<PatientLabOrderResponse?> GetLabOrderDetailsAsync(Guid patientId, Guid orderId);

                /// <summary>
                /// طلب خدمة سحب العينة من البيت
                /// </summary>
                Task<PatientLabOrderResponse> RequestHomeSampleCollectionAsync(Guid patientId, Guid orderId, RequestHomeSampleCollectionRequest request);

                /// <summary>
                /// إلغاء طلب تحاليل
                /// </summary>
                Task<PatientLabOrderResponse> CancelLabOrderAsync(Guid patientId, Guid orderId, string reason);

                #endregion

                #region Lab Results

                /// <summary>
                /// جلب نتائج طلب تحاليل
                /// </summary>
                Task<IEnumerable<PatientLabResultResponse>> GetLabOrderResultsAsync(Guid patientId, Guid orderId);

                #endregion

                #region Reviews

                /// <summary>
                /// إضافة تقييم للمعمل
                /// </summary>
                Task<LaboratoryReviewResponse> CreateLaboratoryReviewAsync(Guid patientId, Guid orderId, CreateLaboratoryReviewRequest request);

                #endregion
        }
}
