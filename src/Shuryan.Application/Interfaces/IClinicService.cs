using Shuryan.Application.DTOs.Requests.Clinic;
using Shuryan.Application.DTOs.Responses.Clinic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Service مسؤول عن إدارة بيانات العيادة (المعلومات، العنوان، الصور)
    /// </summary>
    public interface IClinicService
    {
        #region Clinic Info
        /// <summary>
        /// جلب معلومات العيادة (الاسم، الهواتف، الخدمات)
        /// </summary>
        Task<ClinicInfoResponse?> GetClinicInfoAsync(Guid doctorId);

        /// <summary>
        /// تحديث معلومات العيادة
        /// </summary>
        Task<ClinicInfoResponse> UpdateClinicInfoAsync(Guid doctorId, UpdateClinicInfoRequest request);
        #endregion

        #region Clinic Address
        /// <summary>
        /// جلب عنوان العيادة
        /// </summary>
        Task<ClinicAddressResponse?> GetClinicAddressAsync(Guid doctorId);

        /// <summary>
        /// تحديث عنوان العيادة
        /// </summary>
        Task<ClinicAddressResponse> UpdateClinicAddressAsync(Guid doctorId, UpdateClinicAddressRequest request);
        #endregion

        #region Clinic Images
        /// <summary>
        /// جلب صور العيادة (الحد الأقصى 6 صور)
        /// </summary>
        Task<ClinicImagesListResponse> GetClinicImagesAsync(Guid doctorId);

        /// <summary>
        /// رفع صورة جديدة للعيادة
        /// </summary>
        Task<ClinicImageResponse> UploadClinicImageAsync(Guid doctorId, UploadClinicImageRequest request);

        /// <summary>
        /// حذف صورة من العيادة
        /// </summary>
        Task<bool> DeleteClinicImageAsync(Guid doctorId, Guid imageId);

        /// <summary>
        /// إعادة ترتيب صور العيادة
        /// </summary>
        Task<ClinicImagesListResponse> ReorderClinicImagesAsync(Guid doctorId, ReorderClinicImagesRequest request);
        #endregion
    }
}
