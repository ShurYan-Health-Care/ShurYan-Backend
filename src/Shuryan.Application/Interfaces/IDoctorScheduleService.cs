using Shuryan.Application.DTOs.Requests.Appointment;
using Shuryan.Application.DTOs.Responses.Appointment;
using System;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Service مسؤول عن إدارة جدول المواعيد (الجدول الأسبوعي - المواعيد الاستثنائية)
    /// </summary>
    public interface IDoctorScheduleService
    {
        #region Weekly Schedule
        /// <summary>
        /// جلب الجدول الأسبوعي للدكتور
        /// </summary>
        Task<WeeklyScheduleResponse> GetWeeklyScheduleAsync(Guid doctorId);

        /// <summary>
        /// تحديث الجدول الأسبوعي للدكتور
        /// </summary>
        Task<WeeklyScheduleResponse> UpdateWeeklyScheduleAsync(Guid doctorId, UpdateWeeklyScheduleRequest request);
        #endregion

        #region Exceptional Dates
        /// <summary>
        /// جلب المواعيد الاستثنائية (أيام مفتوحة أو مغلقة بشكل استثنائي)
        /// </summary>
        Task<ExceptionalDatesListResponse> GetExceptionalDatesAsync(Guid doctorId);

        /// <summary>
        /// إضافة موعد استثنائي
        /// </summary>
        Task<ExceptionalDateResponse> AddExceptionalDateAsync(Guid doctorId, AddExceptionalDateRequest request);

        /// <summary>
        /// حذف موعد استثنائي
        /// </summary>
        Task<bool> RemoveExceptionalDateAsync(Guid doctorId, Guid exceptionId);
        #endregion

        #region Frontend Integration - New Format
        /// <summary>
        /// جلب الجدول الأسبوعي بصيغة الفرونت (array of 7 days with dayOfWeek 0-6)
        /// </summary>
        Task<List<DayScheduleSlotResponse>> GetWeeklyScheduleForFrontendAsync(Guid doctorId);

        /// <summary>
        /// جلب المواعيد الاستثنائية بصيغة الفرونت (مع isClosed flag)
        /// </summary>
        Task<List<ExceptionalDateResponse>> GetExceptionalDatesForFrontendAsync(Guid doctorId);
        #endregion
    }
}
