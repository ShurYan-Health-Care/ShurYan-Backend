using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Core.Enums.Appointments;
using System;

namespace Shuryan.Application.DTOs.Requests.Appointment
{
    /// <summary>
    /// Request DTO للحصول على مواعيد الدكتور مع Pagination والفلاتر
    /// </summary>
    public class GetDoctorAppointmentsRequest : PaginationParams
    {
        /// <summary>
        /// تصفية من تاريخ معين (YYYY-MM-DD)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تصفية إلى تاريخ معين (YYYY-MM-DD)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// تصفية حسب حالة الموعد
        /// </summary>
        public AppointmentStatus? Status { get; set; }

        /// <summary>
        /// حقل الترتيب (appointmentDate, patientName, status)
        /// </summary>
        public string SortBy { get; set; } = "appointmentDate";

        /// <summary>
        /// اتجاه الترتيب (asc أو desc)
        /// </summary>
        public string SortOrder { get; set; } = "desc";
    }
}
