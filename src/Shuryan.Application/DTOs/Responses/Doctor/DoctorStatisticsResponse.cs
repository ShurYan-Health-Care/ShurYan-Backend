using System;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    public class DoctorStatisticsResponse
    {
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
    }
}
