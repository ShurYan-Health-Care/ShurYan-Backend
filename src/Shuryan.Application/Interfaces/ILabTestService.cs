using System;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Requests.LabTests;
using Shuryan.Application.DTOs.Responses.LabTests;

namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Service مسؤول عن إدارة طلبات التحاليل الطبية
    /// </summary>
    public interface ILabTestService
    {
        /// <summary>
        /// طلب تحاليل طبية جديدة
        /// </summary>
        Task<LabTestsResponse> RequestLabTestsAsync(Guid appointmentId, Guid doctorId, RequestLabTestsRequest request);

        /// <summary>
        /// الحصول على طلبات التحاليل للموعد
        /// </summary>
        Task<LabTestsResponse?> GetLabTestsAsync(Guid appointmentId, Guid doctorId);
    }
}
