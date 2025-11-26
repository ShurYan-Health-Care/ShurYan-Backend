using Microsoft.AspNetCore.Http;
using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;

namespace Shuryan.Application.Interfaces
{
        public interface ILaboratoryProfileService
        {
                #region Basic Info Operations

                /// جلب المعلومات الأساسية للمعمل
                Task<LaboratoryBasicInfoResponse?> GetBasicInfoAsync(Guid laboratoryId);

                /// تحديث المعلومات الأساسية للمعمل
                Task<LaboratoryBasicInfoResponse> UpdateBasicInfoAsync(Guid laboratoryId, UpdateLaboratoryBasicInfoRequest request);

                /// رفع صورة البروفايل للمعمل
                Task<string> UpdateProfileImageAsync(Guid laboratoryId, IFormFile image);

                #endregion

                #region Address Operations

                /// جلب عنوان المعمل
                Task<LaboratoryAddressResponse?> GetAddressAsync(Guid laboratoryId);

                /// تحديث عنوان المعمل
                Task<LaboratoryAddressResponse> UpdateAddressAsync(Guid laboratoryId, UpdateLaboratoryAddressRequest request);

                #endregion

                #region Working Hours Operations

                /// جلب ساعات وأيام عمل المعمل
                Task<LaboratoryWorkingHoursResponse> GetWorkingHoursAsync(Guid laboratoryId);

                /// تحديث ساعات وأيام عمل المعمل
                Task<LaboratoryWorkingHoursResponse> UpdateWorkingHoursAsync(Guid laboratoryId, UpdateLaboratoryWorkingHoursRequest request);

                #endregion

                #region Home Sample Collection Operations

                /// جلب إعدادات خدمة سحب العينة من البيت
                Task<LaboratoryHomeSampleCollectionResponse> GetHomeSampleCollectionSettingsAsync(Guid laboratoryId);

                /// تحديث إعدادات خدمة سحب العينة من البيت
                Task<LaboratoryHomeSampleCollectionResponse> UpdateHomeSampleCollectionSettingsAsync(Guid laboratoryId, UpdateLaboratoryHomeSampleCollectionRequest request);

                #endregion

                #region Lab Services Operations

                Task<IEnumerable<LabServiceDetailResponse>> GetLabServicesAsync(Guid laboratoryId);
                Task<LabServiceDetailResponse?> GetLabServiceByIdAsync(Guid laboratoryId, Guid serviceId);
                Task<LabServiceDetailResponse> AddLabServiceAsync(Guid laboratoryId, AddLabServiceRequest request);
                Task<LabServiceDetailResponse> UpdateLabServiceAsync(Guid laboratoryId, Guid serviceId, UpdateLabServiceRequest request);
                Task<bool> DeleteLabServiceAsync(Guid laboratoryId, Guid serviceId);
                Task<LabServiceDetailResponse> UpdateLabServiceAvailabilityAsync(Guid laboratoryId, Guid serviceId, UpdateLabServiceAvailabilityRequest request);

                #endregion

                #region Available Lab Tests

                Task<IEnumerable<LabTestListResponse>> GetAvailableLabTestsAsync(string? searchTerm = null, string? category = null);

                #endregion
        }
}
