using Shuryan.Application.DTOs.Requests.Clinic;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.DTOs.Responses.Clinic;
using System;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Service مسؤول عن إدارة أسعار ومدة الخدمات الطبية (كشف عادي - إعادة كشف)
    /// </summary>
    public interface IDoctorServicePricingService
    {
        #region Regular Checkup
        /// <summary>
        /// جلب سعر ومدة الكشف العادي
        /// </summary>
        Task<ServicePricingResponse?> GetRegularCheckupAsync(Guid doctorId);

        /// <summary>
        /// تحديث سعر ومدة الكشف العادي
        /// </summary>
        Task<ServicePricingResponse> UpdateRegularCheckupAsync(Guid doctorId, UpdateServicePricingRequest request);
        #endregion

        #region Re-examination
        /// <summary>
        /// جلب سعر ومدة إعادة الكشف
        /// </summary>
        Task<ServicePricingResponse?> GetReExaminationAsync(Guid doctorId);

        /// <summary>
        /// تحديث سعر ومدة إعادة الكشف
        /// </summary>
        Task<ServicePricingResponse> UpdateReExaminationAsync(Guid doctorId, UpdateServicePricingRequest request);
        #endregion

        #region Frontend Integration
        /// <summary>
        /// جلب كل الخدمات (كشف عادي + إعادة كشف) في response واحد
        /// </summary>
        Task<DoctorServicesResponse> GetAllServicesAsync(Guid doctorId);
        #endregion
    }
}
