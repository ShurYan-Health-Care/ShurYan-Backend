using System;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Responses.Session;

namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Service مسؤول عن إدارة جلسات الكشف (Consultation Sessions)
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// بدء جلسة كشف جديدة
        /// </summary>
        /// <param name="appointmentId">معرف الموعد</param>
        /// <param name="doctorId">معرف الدكتور</param>
        /// <returns>بيانات الجلسة المُنشأة</returns>
        Task<SessionResponse> StartSessionAsync(Guid appointmentId, Guid doctorId);

        /// <summary>
        /// الحصول على الجلسة النشطة للموعد
        /// </summary>
        /// <param name="appointmentId">معرف الموعد</param>
        /// <param name="doctorId">معرف الدكتور</param>
        /// <returns>بيانات الجلسة النشطة أو null</returns>
        Task<SessionResponse?> GetActiveSessionAsync(Guid appointmentId, Guid doctorId);

        /// <summary>
        /// إنهاء الجلسة النشطة
        /// </summary>
        /// <param name="appointmentId">معرف الموعد</param>
        /// <param name="doctorId">معرف الدكتور</param>
        /// <returns>بيانات الجلسة المُنتهية</returns>
        Task<EndSessionResponse> EndSessionAsync(Guid appointmentId, Guid doctorId);

        /// <summary>
        /// الحصول على الجلسة النشطة الحالية للدكتور (أي موعد)
        /// </summary>
        /// <param name="doctorId">معرف الدكتور</param>
        /// <returns>بيانات الجلسة النشطة أو null</returns>
        Task<SessionResponse?> GetDoctorCurrentActiveSessionAsync(Guid doctorId);
    }
}
