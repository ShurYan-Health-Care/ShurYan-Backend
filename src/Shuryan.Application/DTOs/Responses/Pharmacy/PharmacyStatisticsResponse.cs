namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyStatisticsResponse
    {
        public int NewOrdersToday { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public int MonthlyOrders { get; set; }
        public decimal MonthlyRevenue { get; set; }
    }
}
