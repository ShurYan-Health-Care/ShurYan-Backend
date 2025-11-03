using System;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    /// <summary>
    /// Response DTO للإحصائيات الخاصة بالـ Dashboard الخاص بالدكتور
    /// </summary>
    public class DoctorDashboardStatsResponse
    {
        /// <summary>
        /// إجمالي عدد المرضى اللي كشف عليهم الدكتور
        /// </summary>
        public int TotalPatients { get; set; }

        /// <summary>
        /// عدد المواعيد المحجوزة لليوم الحالي
        /// </summary>
        public int TodayAppointments { get; set; }

        /// <summary>
        /// عدد الكشوفات المكتملة (Completed Appointments)
        /// </summary>
        public int CompletedAppointments { get; set; }

        /// <summary>
        /// إجمالي الإيرادات من كل المواعيد المكتملة
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// الإيرادات الشهرية (الشهر الحالي فقط)
        /// </summary>
        public decimal MonthlyRevenue { get; set; }

        /// <summary>
        /// عدد المواعيد المعلقة (Pending/Confirmed)
        /// </summary>
        public int PendingAppointments { get; set; }

        /// <summary>
        /// عدد المواعيد الملغية
        /// </summary>
        public int CancelledAppointments { get; set; }

        /// <summary>
        /// متوسط التقييم للدكتور (من 1 إلى 5)
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// إجمالي عدد التقييمات اللي حصل عليها الدكتور
        /// </summary>
        public int TotalReviews { get; set; }
    }
}
