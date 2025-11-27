namespace Shuryan.Application.DTOs.Responses.Laboratory
{
        /// <summary>
        /// بيانات داشبورد المعمل
        /// </summary>
        public class LaboratoryDashboardResponse
        {
                // Today's Stats
                public int TodayOrdersCount { get; set; }
                public int TodayPendingOrders { get; set; }
                public int TodayCompletedOrders { get; set; }
                public decimal TodayRevenue { get; set; }

                // Overall Stats
                public int TotalOrders { get; set; }
                public int PendingOrders { get; set; }
                public int InProgressOrders { get; set; }
                public int CompletedOrders { get; set; }
                public int CancelledOrders { get; set; }
                public decimal TotalRevenue { get; set; }

                // Recent Orders
                public List<LabOrderResponse> RecentOrders { get; set; } = new();
        }
}
